# AGENTS.md

This file provides guidance to coding agents (Claude Code, GitHub Copilot, etc.) when working with code in this repository.

QACover evaluates the test-data coverage of the SQL queries a Java or .NET application executes, using the *SQL Full Predicate Coverage* (SQLFpc) criterion (and optionally SQL mutation). It intercepts each query execution, generates and evaluates coverage rules, stores results locally, and produces HTML reports. Multi-module Maven project (Java 8+), published to Maven Central under `io.github.giis-uniovi` (`qacover-core`, `qacover-model`) and NuGet (`QACover`, `QACoverReport`).

## How it works (architecture)

At runtime QACover intercepts JDBC calls with [p6spy](https://github.com/p6spy/p6spy), calls [TdRules](https://github.com/giis-uniovi/tdrules) to get the schema and generate the coverage rules, evaluates them against the live connection, and writes results under `target/qacover/`. A key job is locating the application **interaction point** in the call stack: QACover walks the stack from the actual DB call upward, skipping framework packages, using the `qacover.stack.exclusions` / `qacover.stack.inclusions` config.

Two modules, package prefix `giis.qacover.`:
- **qacover-core** — evaluation (`eval`, `eval.coverage`, `eval.query`, `eval.reader`) + interception/integration (`core`, `core.services`, `driver`, `p6spy`). This is the runtime dependency applications add.
- **qacover-model** — model + utilities used for reporting and inspecting rules (`model`, `storage`, `reader`, `report`). Also shipped as a standalone reporter jar.

Dependency direction: interception (`p6spy → driver → core`) drives evaluation (`eval*`), which reads/writes through `storage` against the shared `model`; `report/reader` consume that model. See the README mermaid graphs for the exact package edges.

## Critical: the .NET code is auto-generated from Java

**Java is the source of truth.** Only `p6spy`, `driver`, and `eval.reader` have hand-written .NET implementations; the rest of `net/` is produced from the Java sources by [JavaToCSharp](https://github.com/paulirwin/JavaToCSharp) plus post-processing in `net/build.xml`. Files under `net/**/Translated/` carry the header `THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT` — never edit them; change the Java and regenerate.

```bash
cd net
dotnet tool install JavaToCSharpCli --global   # once
ant convert
dotnet build
```
CI runs `ant convert && dotnet build` on every push to catch Java changes that break the .NET build.

## Build and test (Java)

Run Maven from the repo root. Surefire uses `testFailureIgnore=true`, so `mvn test` exits 0 even on failures — check `*/target/surefire-reports` and the aggregated HTML under `target/site/`. Note `qacover-core` reads snapshot dependencies from the GitHub Packages Maven repos of `giis-uniovi` and `javiertuya` (configured in the root `pom.xml`), which may require a GitHub token.

Tests are partitioned into **scopes** by class-name prefix (see `.github/workflows/test.yml`) because DB-backed tests need a container:
```bash
# UT — no database:
mvn test -Dtest='!TestPostgres*,!TestSqlserver*,!TestOracle*,!IT*' -Dsurefire.failIfNoSpecifiedTests=false
# One DBMS (Postgres / Sqlserver / Oracle):
mvn test -Dtest='TestPostgres*' -Dsurefire.failIfNoSpecifiedTests=false -Duser.timezone=Europe/Madrid
# Single class in one module (-am also builds the sibling modules it depends on):
mvn test -pl qacover-core -am -Dtest=TestConfig
```
`qacover-core` depends on the sibling `qacover-model` (same `-SNAPSHOT` version), so a `-pl qacover-core` build needs `-am` unless `qacover-model` was already installed (`mvn install`); otherwise Maven cannot resolve the snapshot.
`-Duser.timezone=Europe/Madrid` is required for Oracle (avoids `ORA-01882`).

### Local test databases

`setup/container-setup.sh` starts the DBMS containers (postgres:17, mssql 2019, gvenzl/oracle-free). Provide credentials via env vars `TEST_POSTGRES_PWD` / `TEST_SQLSERVER_PWD` / `TEST_ORACLE_PWD`, or a git-ignored `setup/environment.properties`. `setup/database.properties` holds the JDBC config the tests read.

### Integration tests (`it/`)

Integration tests live in `it/` and run **outside** the parent-pom reactor via their own Ant build:
```bash
cd it
ant test-sequential
```
This installs the uber jar, then runs three end-to-end scenarios, each a self-contained project: `spring-petclinic-main` (a Spring Boot SUT depending on `qacover-core`), `qacover-api-sample` (uses `qacover-model` as a read API), and `qacover-uber-main` (a standalone jar using the qacover-core **uber** jar on its classpath). A separate Maven `IT*` test then compares the produced reports against expected results. Windows note: `it/build.xml` auto-selects `cmd`/`bash`.

## Build and test (.NET)

`net/QACover.sln`, netstandard2.0 library, SDK 10. Some tests need SQL Server running (see setup above). There are two interception paths — ADO.NET and Entity Framework:
```bash
cd net
dotnet test QACoverTest/QACoverTest.csproj      # ADO.NET
dotnet test QACoverTestEf/QACoverTestEf.csproj  # Entity Framework
```
- `net/QACover` — the library; `net/QACoverEf2spy` — Entity Framework interceptor; `net/QACoverReport` — the report generator packaged as a `dotnet tool` (net8.0).
- Unlike Java there is no `spy.properties`; interception is activated in code (`EventTrigger.SetListenerClassName(...)` / the `QACOVER_LISTENER_CLASS` env var) or via a `DbConnection`/`DbContext` wrapper.

## Configuration and reports

- **`qacover.properties`** (both platforms) — inclusion/exclusion criteria for locating the interaction point, and general options. Resolved from system properties, then classpath, then the run directory.
- **`spy.properties`** (Java only) — p6spy config; must keep `modulelist=giis.qacover.driver.InterceptorFactory`. On Java the JDBC URL must insert `p6spy` (`jdbc:sqlite:...` → `jdbc:p6spy:sqlite:...`).
- **Reports** are generated from `target/qacover/rules` by the standalone reporter jar (`java -jar qacover-model-<version>-report.jar <rules> <reports> [<sources> [<project>]]`), by `new giis.qacover.report.ReportManager().run(...)`, by `ReportMain` via `exec-maven-plugin`, or by the `QACoverReport` dotnet tool.
- `giis.qacover.eval.StandaloneEvaluator` (Java) evaluates FPC/mutation rules directly from a `TdRules` model without p6spy/config.

## Conventions

- Maven `source`/`target` is **Java 8**; keep sources 8-compatible.
- Public API names match across platforms except casing (Java `run()` ↔ C# `Run()`).
- `.github/workflows/test.yml` uses an `if:` guard on the test jobs to avoid double runs for local-branch PRs while allowing forked-repo and dependabot PRs — preserve it when editing the workflow.
