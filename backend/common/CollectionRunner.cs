using System.Runtime.CompilerServices;
﻿using System.Text;
using common.Bronze;
using common.Silver;
using Microsoft.Extensions.Logging;
using MoreLinq;

namespace common;

public class CollectionRunner
{
    readonly CollectionLocator _locator;
    readonly SeleniumDownloader _downloader;
    readonly FileReader _fileReader;
    readonly Parser _parser;
    readonly FileWriter _fileWriter;
    readonly Hasher _hasher;
    readonly Transformer _transformer;
    readonly ChecksumCalculator _checksumCalculator;
    readonly ILogger _logger;

    public CollectionRunner(
        CollectionLocator locator,
        SeleniumDownloader downloader,
        FileReader fileReader,
        Parser parser,
        FileWriter fileWriter,
        Hasher hasher,
        Transformer transformer,
        ChecksumCalculator checksumCalculator,
        ILogger logger)
    {
        _locator = locator;
        _downloader = downloader;
        _fileReader = fileReader;
        _parser = parser;
        _fileWriter = fileWriter;
        _hasher = hasher;
        _transformer = transformer;
        _checksumCalculator = checksumCalculator;
        _logger = logger;
    }

    public async Task RunLoader(Collection collection, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        foreach (var urls in collection.Urls.Distinct().Batch(10))
        {
            var tasks = new List<Task>();
            foreach (string url in urls)
            {
                string htmlLocation = _locator.GetHtmlLocation(collection.Name, url);
                sb.Append($"{url} {htmlLocation} {Environment.NewLine}");

                if (File.Exists(htmlLocation))
                {
                    _logger.LogInformation("File {htmlLocation} already exist. Skipping...", htmlLocation);
                    continue;
                }

                tasks.Add(Task.Run(() => DownloadAndSave(url, htmlLocation, cancellationToken), cancellationToken));
            }

            await Task.WhenAll(tasks);

            await _fileWriter.AsTextAsync(_locator.GetComponentsFileLocation(collection.Name), sb.ToString(), cancellationToken);
        }
    }

    async Task DownloadAndSave(string url, string htmlLocation, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Downloading from {url} to {htmlLocation}", url, htmlLocation);
        string htmlContent = await _downloader.DownloadAsText(url, cancellationToken);
        await _fileWriter.AsTextAsync(htmlLocation, htmlContent, cancellationToken);
    }

    public async Task RunParser(Collection collection, CancellationToken cancellationToken)
    {
        foreach (string url in collection.Urls.Distinct())
        {
            string htmlLocation = _locator.GetHtmlLocation(collection.Name, url);

            if (!File.Exists(htmlLocation))
            {
                _logger.LogInformation("File {htmlLocation} does not exist. Skipping...", htmlLocation);
                continue;
            }

            string htmlContent = await _fileReader.ReadTextAsync(htmlLocation, cancellationToken);

            string checksum = _checksumCalculator.GetParserChecksum(collection.ParserSchema, htmlContent);
            string checksumLocation = _locator.GetChecksumLocation(collection.Name, url, Medallion.Bronze);
            if (checksum == await GetChecksumAsync(checksumLocation, cancellationToken))
            {
                _logger.LogInformation("Checksum for {htmlLocation} match. Skipping...", htmlLocation);
                continue;
            }

            _logger.LogInformation("Parsing {htmlLocation}", htmlLocation);
            var properties = _parser.Parse(htmlContent, collection.ParserSchema);

            string bronzeFileLocation = _locator.GetDataFileLocation(collection.Name, url, Medallion.Bronze);
            await _fileWriter.AsJsonAsync(bronzeFileLocation, properties, cancellationToken);
            await _fileWriter.AsTextAsync(checksumLocation, checksum, cancellationToken);
        }
    }

    public async Task RunTransformer(Collection collection, CancellationToken cancellationToken)
    {
        foreach (string url in collection.Urls.Distinct())
        {
            string bronzeFileLocation = _locator.GetDataFileLocation(collection.Name, url, Medallion.Bronze);

            if (!File.Exists(bronzeFileLocation)) continue;

            var bronze = await _fileReader.ReadJsonAsync<Data>(bronzeFileLocation, cancellationToken);

            string checksum = _checksumCalculator.GetTransformerChecksum(collection.TransformerSchema, bronze);
            string checksumLocation = _locator.GetChecksumLocation(collection.Name, url, Medallion.Silver);
            if (checksum == await GetChecksumAsync(checksumLocation, cancellationToken))
                continue;

            var silver = _transformer.Transform(bronze, collection.TransformerSchema);

            string silverFileLocation = _locator.GetDataFileLocation(collection.Name, url, Medallion.Silver);
            await _fileWriter.AsJsonAsync(silverFileLocation, silver, cancellationToken);
            await _fileWriter.AsTextAsync(checksumLocation, checksum, cancellationToken);
        }
    }

    async Task<string> GetChecksumAsync(string checksumLocation, CancellationToken cancellationToken)
    {
        return File.Exists(checksumLocation)
            ? await _fileReader.ReadTextAsync(checksumLocation, cancellationToken)
            : string.Empty;
    }
}
