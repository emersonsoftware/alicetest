using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using RefconGatewayBase.Helpers;

namespace RefconGatewayTest.Helpers;

public class ArchiveHandlerTest
{
    private ArchiveHandler sut;

    [SetUp]
    public void Setup()
    {
        sut = new ArchiveHandler();
    }

    [TestCase("REFCON.027", "REFCON.001")]
    [TestCase("TestArchive_1doc.7z", "RefconTestDocument1.txt")]
    public void Extract_OK_ReturnsFile(string attachmentFileName, string refconFileNameExp)
    {
        var attachmentStream = GetAttachmentStream(attachmentFileName);

        var result = sut.Extract(attachmentFileName, attachmentStream);

        // Assert that the result is Dictionary<string,Stream>
        // Assert that the attachment file contained only 1 file after 7zip extraction with expected file name
        Assert.AreEqual(typeof(Dictionary<string, Stream>), result.GetType());
        Assert.AreEqual(1, result.Keys.Count);
        Assert.IsTrue(result.ContainsKey(refconFileNameExp));
    }

    [TestCase("TestArchive_2doc.7z")]
    public void Extract_OK_ReturnsMultipleFiles(string testFileName)
    {
        var attachmentStream = GetAttachmentStream(testFileName);

        var result = sut.Extract(testFileName, attachmentStream);

        // Assert that the attachment file contained multiple files after 7zip extraction
        Assert.AreEqual(2, result.Keys.Count);
    }

    [TestCase("REFCON.001")]
    public void Extract_Fail_InvalidAttachment_ReturnsEmptyDictionary(string invalidfileName)
    {
        // example of invalid file is a refcon file that is not zipped
        var invalidAttachmentStream = GetAttachmentStream(invalidfileName);

        var result = sut.Extract(invalidfileName, invalidAttachmentStream);

        // Assert that 7zip extraction returned no results if attachment file is invalid or empty
        Assert.AreEqual(0, result.Keys.Count);
    }

    #region Helpers

    /// <summary>
    /// Read files from the TestData folder
    /// </summary>
    /// <param name="testFileName"></param>
    /// <returns></returns>
    private Stream GetAttachmentStream(string testFileName)
    {
        var attachmentStream = new MemoryStream();
        using (var fs = File.OpenRead(Directory.GetCurrentDirectory() + "\\TestData\\" + testFileName))
        {
            fs.Position = 0;
            fs.CopyTo(attachmentStream);
        }

        return attachmentStream;
    }

    #endregion Helpers
}