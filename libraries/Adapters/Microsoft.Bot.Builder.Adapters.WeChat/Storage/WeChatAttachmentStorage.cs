﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters.WeChat.Schema.JsonResults;

namespace Microsoft.Bot.Builder.Adapters.WeChat.Storage
{
    public class WeChatAttachmentStorage : IWeChatStorage<WeChatJsonResult>
    {
        public static readonly WeChatAttachmentStorage Instance = new WeChatAttachmentStorage();

        private readonly IStorage _storage;

        public WeChatAttachmentStorage()
        {
            _storage = new MemoryStorage();
        }

        public WeChatAttachmentStorage(IStorage storage)
        {
            _storage = storage;
        }

        public async Task SaveAsync(string key, WeChatJsonResult value, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dict = new Dictionary<string, object>
            {
                { key, value },
            };
            await _storage.WriteAsync(dict, cancellationToken).ConfigureAwait(false);
        }

        public async Task<WeChatJsonResult> GetAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            var keys = new string[] { key };
            var result = await _storage.ReadAsync<WeChatJsonResult>(keys, cancellationToken).ConfigureAwait(false);
            result.TryGetValue(key, out var wechatResult);
            return wechatResult;
        }

        public async Task DeleteAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            var keys = new string[] { key };
            await _storage.DeleteAsync(keys, cancellationToken).ConfigureAwait(false);
        }
    }
}