package giis.qacover.reader;

import java.util.ArrayList;
import java.util.List;
import java.util.SortedMap;
import java.util.TreeMap;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import giis.portable.util.FileUtil;
import giis.portable.util.JavaCs;


/**
 * Collects all info about source code and coverage of queries executed at each
 * line. Indexed by line number, each can contain source code or queries or both
 */
public class SourceCodeCollection {

	private static final Logger log = LoggerFactory.getLogger(SourceCodeCollection.class);

	private TreeMap<Integer, SourceCodeLine> lines = new TreeMap<>();
	
	public SortedMap<Integer, SourceCodeLine> getLines() {
		return lines;
	}
	
	public List<Integer> getLineNumbers() {
		List<Integer> ret = new ArrayList<>();
		for (int key : lines.keySet())
			ret.add(key);
		return ret;
	}

	// Gets the indicated Line object, creates a new if not exists
	private SourceCodeLine getLine(int lineNumber) {
		if (lines.containsKey(lineNumber))
			return lines.get(lineNumber);
		SourceCodeLine newLine = new SourceCodeLine();
		lines.put(lineNumber, newLine);
		return newLine;
	}

	/**
	 * Adds a all queries to this collection. Each will be inserted at the line that
	 * corresponds with the line where the query was run
	 */
	public void addQueries(QueryCollection queries) {
		for (int i = 0; i < queries.size(); i++)
			addQuery(queries.get(i));
	}

	/**
	 * Adds a query to this collection. This will be inserted at the line that
	 * corresponds with the line where the query was run
	 */
	public void addQuery(QueryReader query) {
		int lineNumber = Integer.parseInt(query.getKey().getClassLine());
		SourceCodeLine line = getLine(lineNumber);
		line.getQueries().add(query);
	}
	
	/**
	 * Adds the source file that can be found from the comma separated
	 * list of source folders at the source path indicated by query
	 */
	public void addSources(QueryReader query, String sourceFolders, String projectFolder) {
		String[] locations = JavaCs.splitByChar(sourceFolders, ',');
		for (String sourceFolder : locations) {
			String sourceFile = query.getModel().getSourceLocation();
			String resolvedFile = resolveSourcePath(sourceFolder, projectFolder, sourceFile);
			if ("".equals(resolvedFile)) // skip: empty return means that path can't be resolved
				continue;
			List<String> allLines = FileUtil.fileReadLines(resolvedFile, false);
			if (!JavaCs.isEmpty(allLines)) {
				addSources(allLines);
				return;
			} else {
				log.debug("Source file can't be read or is empty: " + resolvedFile);
			}
		}
		// if sources have not added, no file could found, inform the error
		log.error("Source file can't be read from any of the source folders: " + sourceFolders);
	}

	private void addSources(List<String> sources) {
		for (int i = 0; i < sources.size(); i++) {
			SourceCodeLine line = getLine(i + 1); // line numbers begin with 1
			line.setSource(sources.get(i));
		}
	}

	public String resolveSourcePath(String sourceFolder, String projectFolder, String sourceFile) {
		log.debug("Using project folder - Try to resolve: Source: [" + sourceFolder + "] Project: [" + projectFolder + "] File: [" + sourceFile
				+ "]");
		sourceFolder = JavaCs.isEmpty(sourceFolder) ? "" : sourceFolder.trim();
		projectFolder = JavaCs.isEmpty(projectFolder) ? "" : projectFolder.trim();
		sourceFile = JavaCs.isEmpty(sourceFile) ? "" : sourceFile.trim();

		// Source folder and source file are required, if not, the empty return means that source can't be fount
		if ("".equals(sourceFolder) || "".equals(sourceFile)) {
			log.error("Can not resolve source code location: Source folder or file are empty");
			return "";
		}

		// If projectFolder specified (for net generated coverage), it is expected a full path source File
		// that is converted to relative to projectFolder
		if (!"".equals(projectFolder)) {
			// unifies separators (linux/windows) and simplifies double separators that appear sometimes
			sourceFile = FileUtil.getFullPath(sourceFile.replace("\\", "/")).replace("\\", "/").replace("//", "/");
			String prefix = FileUtil.getFullPath(projectFolder.replace("\\", "/")).replace("\\", "/").replace("//", "/");
			if (!prefix.endsWith("/"))
				prefix = prefix + "/";

			if (sourceFile.startsWith(prefix)) {
				sourceFile = JavaCs.substring(sourceFile, prefix.length(), sourceFile.length());
				log.info("Using project folder - Resolved absolute source file name to relative: " + sourceFile);
			} else {
				log.error("Using project folder - Not resolved: prefix [" + prefix + "] is not part of source file [" + sourceFile + "]");
				return "";
			}
		}
		String resolvedFile = FileUtil.getPath(sourceFolder, sourceFile);
		log.debug("Resolved to: " + resolvedFile);
		return resolvedFile;
	}

}
