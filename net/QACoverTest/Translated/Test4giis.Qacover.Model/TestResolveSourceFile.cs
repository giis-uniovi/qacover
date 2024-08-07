/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using Giis.Portable.Util;
using Giis.Qacover.Reader;
using NUnit.Framework;


namespace Test4giis.Qacover.Model
{
	/// <summary>Resolution of the location path of source filenames</summary>
	public class TestResolveSourceFile
	{
		[Test]
		public virtual void TestNoProjectLocResolved()
		{
			AssertPath("/a/b/c/x/y/Clazz.java", "/a/b/c", string.Empty, "x/y/Clazz.java");
			AssertPath("a/b/c/x/y/Clazz.java", "a/b/c", string.Empty, "x/y/Clazz.java");
			AssertPath("a/b/c/x/y/Clazz.java", "a/b/c/", string.Empty, "x/y/Clazz.java");
			AssertPath("a/b/c/x/y/Clazz.java", "a\\b\\c", string.Empty, "x\\y\\Clazz.java");
			AssertPath("c:/a/b/c/x/y/Clazz.java", "c:\\a\\b\\c", string.Empty, "x\\y\\Clazz.java");
			// current folder
			bool isJava = Parameters.IsJava();
			AssertPath(isJava ? Full("x/y/Clazz.java") : "./x/y/Clazz.java", ".", string.Empty, "x/y/Clazz.java");
			AssertPath(isJava ? Full("x/y/Clazz.java") : "./x/y/Clazz.java", "./", string.Empty, "x/y/Clazz.java");
		}

		[Test]
		public virtual void TestNoProjectLocUnresolved()
		{
			// if no source folder or file, returns empty (no resolved)
			AssertPath(string.Empty, string.Empty, string.Empty, "x/y/Clazz.java");
			AssertPath(string.Empty, null, string.Empty, "x/y/Clazz.java");
			AssertPath(string.Empty, string.Empty, null, "x/y/Clazz.java");
			AssertPath(string.Empty, "a/b/c", string.Empty, string.Empty);
			AssertPath(string.Empty, "a/b/c", string.Empty, null);
		}

		[Test]
		public virtual void TestWithProjectLocAbsoluteResolved()
		{
			AssertPath("/a/b/c/y/Clazz.java", "/a/b/c", "/x", "/x/y/Clazz.java");
			AssertPath("/a/b/c/y/Clazz.java", "/a/b/c", "/x/", "/x/y/Clazz.java");
			AssertPath("/a/b/c/y/Clazz.java", "/a/b/c", "\\x", "/x/y/Clazz.java");
			AssertPath("/a/b/c/y/Clazz.java", "/a/b/c", "\\x\\", "/x/y/Clazz.java");
			AssertPath("/a/b/c/y/Clazz.java", "/a/b/c", "\\x\\", "\\x\\y\\Clazz.java");
		}

		[Test]
		public virtual void TestWithProjectLocAbsoluteUnresolved()
		{
			// project folder is not included in full path
			AssertPath(string.Empty, "/a/b/c", "/w", "/x/y/Clazz.java");
		}

		[Test]
		public virtual void TestWithProjectLocRelativeResolved()
		{
			AssertPath("/a/b/c/y/Clazz.java", "/a/b/c", "x", FileUtil.GetFullPath("x/y/Clazz.java"));
			AssertPath("/a/b/c/y/Clazz.java", "/a/b/c", "x/", FileUtil.GetFullPath("x/y/Clazz.java"));
			AssertPath("/a/b/c/y/Clazz.java", "/a/b/c", "./x", FileUtil.GetFullPath("x/y/Clazz.java"));
			// current folder
			AssertPath("/a/b/c/x/y/Clazz.java", "/a/b/c", ".", FileUtil.GetFullPath("x/y/Clazz.java"));
			AssertPath("/a/b/c/x/y/Clazz.java", "/a/b/c", "./", FileUtil.GetFullPath("x/y/Clazz.java"));
		}

		[Test]
		public virtual void TestWithProjectLocRelativeUnresolved()
		{
			// project folder is not included in full path
			AssertPath(string.Empty, "/a/b/c", "w", FileUtil.GetFullPath("x/y/Clazz.java"));
		}

		private void AssertPath(string expected, string source, string project, string file)
		{
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(expected, new SourceCodeCollection().ResolveSourcePath(source, project, file).Replace("\\", "/"));
		}

		private string Full(string filename)
		{
			return FileUtil.GetFullPath(filename).Replace("\\", "/");
		}
	}
}
