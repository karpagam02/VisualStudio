﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using GitHub.Models;
using GitHub.Services;

namespace GitHub.InlineReviews.Services
{
    /// <summary>
    /// Provides a common interface for services required by <see cref="PullRequestSession"/>.
    /// </summary>
    [Export(typeof(IPullRequestSessionService))]
    class PullRequestSessionService : IPullRequestSessionService
    {
        readonly IGitService gitService;
        readonly IGitClient gitClient;
        readonly IDiffService diffService;

        [ImportingConstructor]
        public PullRequestSessionService(
            IGitService gitService,
            IGitClient gitClient,
            IDiffService diffService)
        {
            this.gitService = gitService;
            this.gitClient = gitClient;
            this.diffService = diffService;
        }

        /// <inheritdoc/>
        public Task<IList<DiffChunk>> Diff(ILocalRepositoryModel repository, string baseSha, string relativePath, byte[] contents)
        {
            var repo = gitService.GetRepository(repository.LocalPath);
            return diffService.Diff(repo, baseSha, relativePath, contents);
        }

        /// <inheritdoc/>
        public string GetTipSha(ILocalRepositoryModel repository)
        {
            var repo = gitService.GetRepository(repository.LocalPath);
            return repo.Head.Tip.Sha;
        }

        /// <inheritdoc/>
        public async Task<bool> IsUnmodifiedAndPushed(ILocalRepositoryModel repository, string relativePath, byte[] contents)
        {
            var repo = gitService.GetRepository(repository.LocalPath);

            return !await gitClient.IsModified(repo, relativePath, contents) &&
                   await gitClient.IsHeadPushed(repo);
        }

        /// <inheritdoc/>
        public async Task<byte[]> ReadFileAsync(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                    {
                        var buffer = new MemoryStream();
                        await file.CopyToAsync(buffer);
                        return buffer.ToArray();
                    }
                }
                catch { }
            }

            return null;
        }
    }
}