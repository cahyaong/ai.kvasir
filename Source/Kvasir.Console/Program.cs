// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2020 Cahya Ong
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
// </copyright>
// <author>Cahya Ong - cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, March 28, 2020 6:00:41 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Console
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.AI.Kvasir.Core;
    using nGratis.AI.Kvasir.Core.Parser;
    using nGratis.Cop.Olympus.Contract;
    using nGratis.Cop.Olympus.Framework;

    public class Program
    {
        private static void Main()
        {
            var dataFolderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "NGRATIS",
                "ai.kvasir");

            // TODO: Use <Unity> to wire dependency injection!

            var indexManager = new IndexManager(new Uri(dataFolderPath));
            var fetcher = new NopFetcher();
            var unprocessedRepository = new UnprocessedMagicRepository(indexManager, fetcher);

            var dataStorageManager = new FileStorageManager(Program.FindDataFolderUri());

            using var processedStorageManager = new CompressedStorageManager(
                new DataSpec("Processed_Data", KvasirMime.Cache),
                dataStorageManager);

            var processedRepository = new ProcessedMagicRepository(processedStorageManager);

            var logger = new ConsoleLogger("CardProcessing");

            var processingExecutor = new ProcessingCardExecution(
                unprocessedRepository,
                processedRepository,
                MagicCardProcessor.Instance,
                logger);

            var processingParameter = ExecutionParameter.Builder
                .Create()
                .WithEntry("CardSet.Name", "Portal")
                .Build();

            Task.WaitAll(processingExecutor.ExecuteAsync(processingParameter));

            Console.WriteLine();
            Console.WriteLine("Press <ANY> key to continue...");
            Console.ReadLine();
        }

        private static Uri FindDataFolderUri()
        {
            var dataFolderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "NGRATIS",
                "ai.kvasir");

            if (!Directory.Exists(dataFolderPath))
            {
                Directory.CreateDirectory(dataFolderPath);
            }

            return new Uri(dataFolderPath);
        }
    }
}