using System;
using System.Collections.Generic;
using System.IO;
using MimeKit;
using NSubstitute;
using NUnit.Framework;
using RefconGatewayBase.Contracts;
using RefconGatewayBase.Mail;
using RefconGatewayBase.Services;
using RefconGatewayBase.Storage.Blobs;

namespace RefconGatewayTest.Services;

public class RefconStorageServiceTest
{
    private IBlobService blobServiceMock;
    private RefconStorageService sut;

    [SetUp]
    public void Setup()
    {
        blobServiceMock = Substitute.For<IBlobService>();

        sut = new RefconStorageService(blobServiceMock);
    }

    #region Attachments

    [Test]
    public void SaveEmailAttachment_OK()
    {
        var entity = CreateMimePart();
        var attachmentSummary = new RefconAttachmentSummary { EmailSummary = CreateRefconEmailMessageSummary() };
        blobServiceMock.StoreFileAsync(Arg.Any<MemoryStream>(), Arg.Any<string>(), Arg.Any<string>()).Returns("testURI");

        var result = sut.SaveEmailAttachment(entity, attachmentSummary);

        blobServiceMock.Received(1).StoreFileAsync(Arg.Any<MemoryStream>(), Arg.Any<string>(), Arg.Any<string>());
        Assert.AreEqual("testURI", result);
    }

    [Test]
    public void SaveEmailAttachmentWithRetry_OK_NoRetryOnSuccess()
    {
        var entity = CreateMimePart();
        var attachmentSummary = new RefconAttachmentSummary { EmailSummary = CreateRefconEmailMessageSummary() };
        blobServiceMock.StoreFileAsync(Arg.Any<MemoryStream>(), Arg.Any<string>(), Arg.Any<string>()).Returns("testURI");

        var result = sut.SaveEmailAttachmentWithRetry(entity, attachmentSummary);

        blobServiceMock.Received(1).StoreFileAsync(Arg.Any<MemoryStream>(), Arg.Any<string>(), Arg.Any<string>());
        Assert.AreEqual("testURI", result);
    }

    [Test]
    public void SaveEmailAttachmentWithRetry_MaxRetryExceeded()
    {
        var entity = CreateMimePart();
        var attachmentSummary = new RefconAttachmentSummary { EmailSummary = CreateRefconEmailMessageSummary() };
        // mock blob client exception
        blobServiceMock.When(x => x.StoreFileAsync(Arg.Any<MemoryStream>(), Arg.Any<string>(), Arg.Any<string>())).Do(x => { throw new TimeoutException(); });

        var result = sut.SaveEmailAttachmentWithRetry(entity, attachmentSummary);

        blobServiceMock.Received(6).StoreFileAsync(Arg.Any<MemoryStream>(), Arg.Any<string>(), Arg.Any<string>());
        Assert.AreEqual("", result);
    }

    #endregion Attachments

    #region Helpers

    private MimePart CreateMimePart()
    {
        return new MimePart
        {
            Content = new MimeContent(new MemoryStream(RandomBufferGenerator(10)))
        };
    }

    private RefconEmailMessageSummary CreateRefconEmailMessageSummary()
    {
        return new RefconEmailMessageSummary
        {
            UniqueId = "uniqueId",
            From = new List<string> { "testSender1", "testSender2" },
            Sent = DateTime.UtcNow,
            Subject = "REFCON test file"
        };
    }

    private byte[] RandomBufferGenerator(int maxBytes)
    {
        var rnd = new Random();
        var b = new byte[maxBytes];
        rnd.NextBytes(b);

        return b;
    }

    #endregion Helpers
}