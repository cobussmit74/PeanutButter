﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.FileSystem.Tests
{
    [TestFixture]
    public class TestLocalFileSystem
    {
        [Test]
        public void Type_ShouldImplent_IFileSystem()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(LocalFileSystem);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldImplement<IFileSystem>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => new LocalFileSystem());

            //---------------Test Result -----------------------
        }

        [Test]
        public void List_GivenNoArguments_ShouldListAllFilesAndFoldersInGivenPath()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var sub1 = "folder" + GetRandomString();
                var sub2 = "folder" + GetRandomString();
                var file1 = "file" + GetRandomString();
                var file2 = "file" + GetRandomString();

                Directory.CreateDirectory(Path.Combine(folder.Path, sub1));
                Directory.CreateDirectory(Path.Combine(folder.Path, sub2));
                File.WriteAllBytes(Path.Combine(folder.Path, file1), GetRandomBytes());
                File.WriteAllBytes(Path.Combine(folder.Path, file2), GetRandomBytes());

                var sut = Create();

                //---------------Assert Precondition----------------
                Assert.IsTrue(Directory.Exists(Path.Combine(folder.Path, sub1)));
                Assert.IsTrue(Directory.Exists(Path.Combine(folder.Path, sub1)));
                Assert.IsTrue(File.Exists(Path.Combine(folder.Path, file1)));
                Assert.IsTrue(File.Exists(Path.Combine(folder.Path, file2)));

                //---------------Execute Test ----------------------
                var result = sut.List(folder.Path);

                //---------------Test Result -----------------------
                CollectionAssert.Contains(result, sub1);
                CollectionAssert.Contains(result, sub2);
                CollectionAssert.Contains(result, file1);
                CollectionAssert.Contains(result, file2);
            }
        }

        [Test]
        public void List_GivenSearchPattern_ShouldReturnOnlyMatchingFoldersAndFiles()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var matchPrefix = GetRandomString(3,5);
                var nonMatchPrefix = GetAnother(matchPrefix);
                var matchFolder = matchPrefix + GetRandomString();
                var matchFile = matchPrefix + GetRandomString();
                var nonMatchFolder = nonMatchPrefix + GetRandomString();
                var nonMatchFile = nonMatchPrefix + GetRandomString();

                Directory.CreateDirectory(Path.Combine(folder.Path, matchFolder));
                Directory.CreateDirectory(Path.Combine(folder.Path, nonMatchFolder));
                File.WriteAllBytes(Path.Combine(folder.Path, matchFile), GetRandomBytes());
                File.WriteAllBytes(Path.Combine(folder.Path, nonMatchFile), GetRandomBytes());
                var sut = Create();

                //---------------Assert Precondition----------------
                Assert.IsTrue(Directory.Exists(Path.Combine(folder.Path, matchFolder)));
                Assert.IsTrue(Directory.Exists(Path.Combine(folder.Path, nonMatchFolder)));
                Assert.IsTrue(File.Exists(Path.Combine(folder.Path, matchFile)));
                Assert.IsTrue(File.Exists(Path.Combine(folder.Path, nonMatchFile)));

                //---------------Execute Test ----------------------
                var result = sut.List(folder.Path, matchPrefix + "*");

                //---------------Test Result -----------------------
                CollectionAssert.Contains(result, matchFolder);
                CollectionAssert.Contains(result, matchFile);
                CollectionAssert.DoesNotContain(result, nonMatchFolder);
                CollectionAssert.DoesNotContain(result, nonMatchFile);
            }
        }

        [Test]
        public void List_GivenPathAndFileExtensionMatch_ShouldFineMatchingFiles()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var ext = GetRandomString(3,3);
                var otherExt = GetAnother(ext, () => GetRandomString(3,3));
                var matchFiles = GetRandomCollection(() => GetRandomString(2,4) + "." + ext, 3, 5);
                var nonMatchFiles = GetRandomCollection(() => GetRandomString(2,4) + "." + otherExt, 3, 5);
                matchFiles.Union(nonMatchFiles).ForEach(f =>
                    File.WriteAllBytes(Path.Combine(folder.Path, f), GetRandomBytes()));
                var sut = Create();
                //---------------Assert Precondition----------------
                Assert.IsTrue(matchFiles.Union(nonMatchFiles).All(f => File.Exists(Path.Combine(folder.Path, f))));

                //---------------Execute Test ----------------------
                var result = sut.List(folder.Path, "*." + ext);

                //---------------Test Result -----------------------
                Assert.IsTrue(matchFiles.All(f => result.Contains(f)));
                Assert.IsFalse(nonMatchFiles.Any(f => result.Contains(f)));
            }
        }

        [Test]
        public void ListFiles_GivenPath_ShouldReturnOnlyFilesFromPath()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var expected = GetRandomString();
                var unexpected = GetAnother(expected);
                Directory.CreateDirectory(Path.Combine(folder.Path, unexpected));
                File.WriteAllBytes(Path.Combine(folder.Path, expected), GetRandomBytes());
                var sut = Create();
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = sut.ListFiles(folder.Path);

                //---------------Test Result -----------------------
                CollectionAssert.Contains(result, expected);
                CollectionAssert.DoesNotContain(result, unexpected);
            }
        }


        [Test]
        public void ListFiles_GivenPathAndSearchPattern_ShouldReturnListOfMatchingFilesInPath()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var ext = GetRandomString(3,3);
                var otherExt = GetAnother(ext, () => GetRandomString(3,3));
                var matchFiles = GetRandomCollection(() => GetRandomString(2,4) + "." + ext, 3, 5);
                var nonMatchFiles = GetRandomCollection(() => GetRandomString(2,4) + "." + otherExt, 3, 5);
                matchFiles.Union(nonMatchFiles).ForEach(f =>
                    File.WriteAllBytes(Path.Combine(folder.Path, f), GetRandomBytes()));
                var unexpected = GetAnother(matchFiles, () => GetRandomString(2,4) + "." + ext);
                Directory.CreateDirectory(Path.Combine(folder.Path, unexpected));
                var sut = Create();
                //---------------Assert Precondition----------------
                Assert.IsTrue(matchFiles.Union(nonMatchFiles).All(f => File.Exists(Path.Combine(folder.Path, f))));

                //---------------Execute Test ----------------------
                var result = sut.ListFiles(folder.Path, "*." + ext);

                //---------------Test Result -----------------------
                Assert.IsTrue(matchFiles.All(f => result.Contains(f)));
                Assert.IsFalse(result.Contains(unexpected));
                Assert.IsFalse(nonMatchFiles.Any(f => result.Contains(f)));
            }
        }


        [Test]
        public void ListFolders_GivenPath_ShouldReturnOnlyFoldersFromPath()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var expected = GetRandomString();
                var unexpected = GetAnother(expected);
                Directory.CreateDirectory(Path.Combine(folder.Path, expected));
                File.WriteAllBytes(Path.Combine(folder.Path, unexpected), GetRandomBytes());
                var sut = Create();
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = sut.ListFolders(folder.Path);

                //---------------Test Result -----------------------
                CollectionAssert.Contains(result, expected);
                CollectionAssert.DoesNotContain(result, unexpected);
            }
        }


        [Test]
        public void ListFolders_GivenPathAndSearchPattern_ShouldReturnListOfMatchingFoldersInPath()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var ext = GetRandomString(3, 3);
                var otherExt = GetAnother(ext, () => GetRandomString(3, 3));
                var matchFolders = GetRandomCollection(() => GetRandomString(2, 4) + "." + ext, 3, 5);
                var nonMatchFolders = GetRandomCollection(() => GetRandomString(2, 4) + "." + otherExt, 3, 5);
                matchFolders.Union(nonMatchFolders).ForEach(f =>
                    Directory.CreateDirectory(Path.Combine(folder.Path, f)));
                var unexpected = GetAnother(matchFolders, () => GetRandomString(2, 4) + "." + ext);
                File.WriteAllBytes(Path.Combine(folder.Path, unexpected), GetRandomBytes());
                var sut = Create();
                //---------------Assert Precondition----------------
                Assert.IsTrue(matchFolders.Union(nonMatchFolders).All(f => Directory.Exists(Path.Combine(folder.Path, f))));

                //---------------Execute Test ----------------------
                var result = sut.ListFolders(folder.Path, "*." + ext);

                //---------------Test Result -----------------------
                Assert.IsTrue(matchFolders.All(f => result.Contains(f)));
                Assert.IsFalse(result.Contains(unexpected));
                Assert.IsFalse(nonMatchFolders.Any(f => result.Contains(f)));
            }
        }

        [Test]
        public void ListFilesRecursive_GivenPath_ShouldReturnOnlyFilesFromPathAndBelow()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var expected = GetRandomString();
                var unexpected = GetAnother(expected);
                Directory.CreateDirectory(Path.Combine(folder.Path, unexpected));
                var expected2 = Path.Combine(unexpected, GetRandomString());
                File.WriteAllBytes(Path.Combine(folder.Path, expected), GetRandomBytes());
                File.WriteAllBytes(Path.Combine(folder.Path, expected2), GetRandomBytes());
                var sut = Create();
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = sut.ListFilesRecursive(folder.Path);

                //---------------Test Result -----------------------
                CollectionAssert.Contains(result, expected);
                CollectionAssert.Contains(result, expected2);
                CollectionAssert.DoesNotContain(result, unexpected);
            }
        }


        [Test]
        public void ListFilesRecursive_GivenPathAndSearchPattern_ShouldReturnListOfMatchingFilesInPath()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var ext = GetRandomString(3, 3);
                var otherExt = GetAnother(ext, () => GetRandomString(3, 3));
                var matchFiles = GetRandomCollection(() => GetRandomString(2, 4) + "." + ext, 3, 5);
                var nonMatchFiles = GetRandomCollection(() => GetRandomString(2, 4) + "." + otherExt, 3, 5);
                var subMatchFiles = new List<string>();
                matchFiles.ForEach(f =>
                {
                    var subFolder = GetRandomString();
                    Directory.CreateDirectory(Path.Combine(folder.Path, subFolder));
                    var subPath = Path.Combine(subFolder, f);
                    subMatchFiles.Add(subPath);
                    File.WriteAllBytes(Path.Combine(folder.Path, subPath), GetRandomBytes());
                });
                var subNonMatchFiles = new List<string>();
                nonMatchFiles.ForEach(f =>
                {
                    var subFolder = GetRandomString();
                    Directory.CreateDirectory(Path.Combine(folder.Path, subFolder));
                    var subPath = Path.Combine(subFolder, f);
                    subNonMatchFiles.Add(subPath);
                    File.WriteAllBytes(Path.Combine(folder.Path, subPath), GetRandomBytes());
                });
                var unexpected = GetAnother(matchFiles, () => GetRandomString(2, 4) + "." + ext);
                Directory.CreateDirectory(Path.Combine(folder.Path, unexpected));
                var sut = Create();
                //---------------Assert Precondition----------------
                Assert.IsTrue(subMatchFiles.Union(subNonMatchFiles)
                                .All(f => File.Exists(Path.Combine(folder.Path, f))));

                //---------------Execute Test ----------------------
                var result = sut.ListFilesRecursive(folder.Path, "*." + ext);

                //---------------Test Result -----------------------
                Assert.IsTrue(subMatchFiles.All(f => result.Contains(f)));
                Assert.IsFalse(result.Contains(unexpected));
                Assert.IsFalse(subNonMatchFiles.Any(f => result.Contains(f)));
            }
        }


        private IFileSystem Create()
        {
            return new LocalFileSystem();
        }
    }
}
