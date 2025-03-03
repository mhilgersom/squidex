﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Assets;

namespace TestSuite.Utils;

public sealed class PauseStream(Stream innerStream, double pauseAfter) : DelegateStream(innerStream)
{
    private readonly int maxLength = (int)Math.Floor(innerStream.Length * pauseAfter) + 1;
    private long totalRead;
    private long totalRemaining = innerStream.Length;
    private long seekStart;

    public override long Length
    {
        get => Math.Min(maxLength, totalRemaining);
    }

    public override long Position
    {
        get => base.Position - seekStart;
        set => throw new NotSupportedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        var position = seekStart = base.Seek(offset, origin);

        totalRemaining = base.Length - position;

        return position;
    }

    public void Reset()
    {
        totalRead = 0;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer,
        CancellationToken cancellationToken = default)
    {
        var remaining = Length - totalRead;

        if (remaining <= 0)
        {
            return 0;
        }

        if (remaining < buffer.Length)
        {
            var remainingBytes = (int)remaining;

            buffer = buffer[..remainingBytes];
        }

        var bytesRead = await base.ReadAsync(buffer, cancellationToken);

        totalRead += bytesRead;

        return bytesRead;
    }
}
