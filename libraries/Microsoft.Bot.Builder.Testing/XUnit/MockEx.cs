﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Moq;

namespace Microsoft.Bot.Builder.Testing.XUnit
{
    /// <summary>
    /// Extension methods for <see cref="Mock"/>.
    /// </summary>
    public static class MockEx
    {
        public static void SetupRecognizeAsync<T>(this Mock<IRecognizer> mockRecognizer, T returns)
            where T : IRecognizerConvert, new()
        {
            mockRecognizer
                .Setup(x => x.RecognizeAsync<T>(It.IsAny<ITurnContext>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(returns));
        }
    }
}
