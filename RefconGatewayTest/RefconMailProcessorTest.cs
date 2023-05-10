using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Imap;
using MimeKit;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using RefconGatewayBase.Contracts;
using RefconGatewayBase.Mail;
using RefconGatewayBase.Peripherals;
using RefconGatewayBase.Services;

namespace RefconGatewayTest;

public class RefconMailProcessorTest
{
    private IImapClient clientMock;
    private IQueueMessageHandler messageHandlerMock;
    private IRefconStorageService storageServiceMock;

    private RefconMailProcessor sut;

    [SetUp]
    public void Setup()
    {
        messageHandlerMock = Substitute.For<IQueueMessageHandler>();
        storageServiceMock = Substitute.For<IRefconStorageService>();
        clientMock = Substitute.For<IImapClient>();

        sut = new RefconMailProcessor(storageServiceMock, messageHandlerMock);
    }

    [Test]
    public async Task ProcessAsync_OK_NoAttachments()
    {
        var summaryMessage = new MessageSummary(0)
        {
            UniqueId = new UniqueId(1),
            Envelope = new Envelope { Subject = "testSubject", Date = DateTimeOffset.Now }
        };

        var filePath = "";
        var entity = new MimePart();
        storageServiceMock.SaveEmailAttachment(Arg.Any<MimeEntity>(), Arg.Any<RefconAttachmentSummary>()).Returns(filePath);
        messageHandlerMock.SendAsync(Arg.Any<RefconQueueMessage>()).Returns(Task.FromResult);
        clientMock.Inbox.AddFlagsAsync(Arg.Any<UniqueId>(), MessageFlags.Deleted, Arg.Any<bool>()).Returns(Task.FromResult);

        await sut.ProcessAsync(clientMock, summaryMessage);

        storageServiceMock.Received(0).SaveEmailAttachment(Arg.Any<MimeEntity>(), Arg.Any<RefconAttachmentSummary>());
        await messageHandlerMock.Received(0).SendAsync(Arg.Any<RefconQueueMessage>());
        await clientMock.Inbox.Received(1).AddFlagsAsync(Arg.Any<UniqueId>(), MessageFlags.Deleted, Arg.Any<bool>());
    }

    [Test]
    public async Task Process_Async_OK_Attachment()
    {
        // create message summary data which must be passed into the method in test. Include an attachment summary (BodyPartBasic)
        var summaryMessage = new MessageSummary(0)
        {
            UniqueId = new UniqueId(1),
            Envelope = new Envelope { Subject = "testSubject", Date = DateTimeOffset.Now },
            Body = new BodyPartBasic
            {
                ContentDescription = "test refcon attachment",
                ContentDisposition = new ContentDisposition("attachment") { FileName = "TestFile.dat" },
                Octets = 4 // just need some value greater than 0, meaning the attachment is not empty
            }
        };

        // mock the data when we call ImapClient to get the attachment from the server
        var entity = new MimePart { Content = new MimeContent(new MemoryStream()) };
        clientMock.Inbox.GetBodyPartAsync(Arg.Any<UniqueId>(), Arg.Any<BodyPartBasic>()).Returns(Task.FromResult<MimeEntity>(entity));

        // mock the data for blob file names that we expected to have created. one for each attachment in the message summary
        var filePath = $"attachment: {summaryMessage.Attachments.First().ContentDescription}";
        storageServiceMock.SaveEmailAttachment(Arg.Any<MimeEntity>(), Arg.Any<RefconAttachmentSummary>()).Returns(filePath);

        // void tasks
        messageHandlerMock.SendAsync(Arg.Any<RefconQueueMessage>()).Returns(Task.FromResult);
        clientMock.Inbox.AddFlagsAsync(Arg.Any<UniqueId>(), MessageFlags.Deleted, Arg.Any<bool>()).Returns(Task.FromResult);

        await sut.ProcessAsync(clientMock, summaryMessage);

        storageServiceMock.Received(1).SaveEmailAttachment(Arg.Any<MimeEntity>(), Arg.Any<RefconAttachmentSummary>());
        await messageHandlerMock.Received(1).SendAsync(Arg.Any<RefconQueueMessage>());
        await clientMock.Inbox.Received(1).AddFlagsAsync(Arg.Any<UniqueId>(), MessageFlags.Deleted, Arg.Any<bool>());
    }

    [Test]
    public async Task ProcessAsync_OK_AttachmentError()
    {
        var summaryMessage = new MessageSummary(0)
        {
            UniqueId = new UniqueId(1),
            Envelope = new Envelope { Subject = "testSubject", Date = DateTimeOffset.Now },
            Body = new BodyPartBasic
            {
                ContentDescription = "test refcon attachment",
                ContentDisposition = new ContentDisposition("attachment") { FileName = "TestFile.dat" },
                Octets = 4 // just need some value greater than 0, meaning the attachment is not empty
            }
        };

        // mock the data when we call ImapClient to get the attachment from the server
        var entity = new MimePart { Content = new MimeContent(new MemoryStream()) };
        clientMock.Inbox.GetBodyPartAsync(Arg.Any<UniqueId>(), Arg.Any<BodyPartBasic>()).Returns(Task.FromResult<MimeEntity>(entity));

        // mock the data for blob file names that we expected to have created. one for each attachment in the message summary
        var filePath = $"attachment: {summaryMessage.Attachments.First().ContentDescription}";

        storageServiceMock
            .SaveEmailAttachment(Arg.Any<MimeEntity>(), Arg.Any<RefconAttachmentSummary>())
            .Throws(new Exception("Internal error"));
        await sut.ProcessAsync(clientMock, summaryMessage);
    }
}