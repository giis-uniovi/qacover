package test4giis.qacover.model;

import static org.junit.Assert.assertEquals;

import org.junit.Test;

import giis.portable.util.FileUtil;
import giis.qacover.reader.SourceCodeCollection;

/**
 * Resolution of the location path of source filenames
 */
public class TestResolveSourceFile {

	@Test
	public void testNoProjectLocResolved() {
		assertPath("/a/b/c/x/y/Clazz.java", "/a/b/c", "", "x/y/Clazz.java");
		assertPath("a/b/c/x/y/Clazz.java", "a/b/c", "", "x/y/Clazz.java");
		assertPath("a/b/c/x/y/Clazz.java", "a/b/c/", "", "x/y/Clazz.java");
		assertPath("a/b/c/x/y/Clazz.java", "a\\b\\c", "", "x\\y\\Clazz.java");
		assertPath("c:/a/b/c/x/y/Clazz.java", "c:\\a\\b\\c", "", "x\\y\\Clazz.java");

		// current folder
		boolean isJava = giis.portable.util.Parameters.isJava();
		assertPath(isJava ? full("x/y/Clazz.java") : "./x/y/Clazz.java", ".", "", "x/y/Clazz.java");
		assertPath(isJava ? full("x/y/Clazz.java") : "./x/y/Clazz.java", "./", "", "x/y/Clazz.java");
	}

	@Test
	public void testNoProjectLocUnresolved() {
		// if no source folder or file, returns empty (no resolved)
		assertPath("", "", "", "x/y/Clazz.java");
		assertPath("", null, "", "x/y/Clazz.java");
		assertPath("", "", null, "x/y/Clazz.java");
		assertPath("", "a/b/c", "", "");
		assertPath("", "a/b/c", "", null);
	}

	@Test
	public void testWithProjectLocAbsoluteResolved() {
		assertPath("/a/b/c/y/Clazz.java", "/a/b/c", "/x", "/x/y/Clazz.java");
		assertPath("/a/b/c/y/Clazz.java", "/a/b/c", "/x/", "/x/y/Clazz.java");
		assertPath("/a/b/c/y/Clazz.java", "/a/b/c", "\\x", "/x/y/Clazz.java");
		assertPath("/a/b/c/y/Clazz.java", "/a/b/c", "\\x\\", "/x/y/Clazz.java");
		assertPath("/a/b/c/y/Clazz.java", "/a/b/c", "\\x\\", "\\x\\y\\Clazz.java");
	}

	@Test
	public void testWithProjectLocAbsoluteUnresolved() {
		// project folder is not included in full path
		assertPath("", "/a/b/c", "/w", "/x/y/Clazz.java");
	}

	@Test
	public void testWithProjectLocRelativeResolved() {
		assertPath("/a/b/c/y/Clazz.java", "/a/b/c", "x", FileUtil.getFullPath("x/y/Clazz.java"));
		assertPath("/a/b/c/y/Clazz.java", "/a/b/c", "x/", FileUtil.getFullPath("x/y/Clazz.java"));
		assertPath("/a/b/c/y/Clazz.java", "/a/b/c", "./x", FileUtil.getFullPath("x/y/Clazz.java"));

		// current folder
		assertPath("/a/b/c/x/y/Clazz.java", "/a/b/c", ".", FileUtil.getFullPath("x/y/Clazz.java"));
		assertPath("/a/b/c/x/y/Clazz.java", "/a/b/c", "./", FileUtil.getFullPath("x/y/Clazz.java"));
	}

	@Test
	public void testWithProjectLocRelativeUnresolved() {
		// project folder is not included in full path
		assertPath("", "/a/b/c", "w", FileUtil.getFullPath("x/y/Clazz.java"));
	}

	private void assertPath(String expected, String source, String project, String file) {
		assertEquals(expected, new SourceCodeCollection().resolveSourcePath(source, project, file).replace("\\", "/"));
	}

	private String full(String filename) {
		return FileUtil.getFullPath(filename).replace("\\", "/");
	}

}
