# AutoVersionIt

## Summary
AutoVersionIt is a little cross-platform tool for automatic versioning that supports reading and updating version information on various sources and destinations.
It offers a flexible approach to managing version numbers on CI/CD pipelines by allowing you to specify which Versioning strategy to use.
It is designed to be straightforward to use. Everything is configured in a JSON file that can be easily committed to your repository.
And can also be used to automatically update the version information in your various repository files.

## Motivation
While working on DevOps processes, I became frustrated by the lack of stable and easy-to-use support for automatic versioning in the .NET ecosystem.
Most existing solutions were either overly complex or lacked the robustness and simplicity that I need for modern CI/CD pipelines.
I was using the awesome [Assembly Info](https://github.com/BMuuN/vsts-assemblyinfo-task) extension for Azure DevOps for a while,
but there is no equivalent for GitHub Actions or other CI/CD systems. And definitely the Telemetry functionality raised me more than one eyebrow from the security department.

AutoVersionIt aims to solve these pain points by providing a straightforward, local-only, cross-platform, and configurable approach to version management.

## Patch options:
AutoVersionIt can patch the version information contained in the following file types:

- C#/VB .NET Framework project types (AssemblyInfo.cs/.vb)
- C#/VB .NET Core project types (ProjectName.csproj/.vbproj)
- NuSpec package definitions (*.nuspec)
- Plain text files (*.txt)

## Sources:
AutoVersionIt can read version information from various sources, including:

- Git tags (using the GIT command line client)
- Environment variables
- Plain text files

## Targets:
AutoVersionIt can write updated version information to various targets, including:

- Git tags (using the GIT command line client)
- Environment variables
- Plain text files

## Dependencies
AutoVersionIt is built on .NET 8.0 and depends only on a few NuGet packages and the GIT command line client in case you need to read/write version details from/to a Git tag.

- .NET 8.0 Runtime
- Git (for Git-based version sources)

## Building
To build AutoVersionIt, you need to have the .NET 8.0 SDK installed. If you are here, you know how to do it.
- Then build the application with the following command:
```shell
  dotnet publish ./AutoVersionIt/AutoVersionIt.csproj -c Release -r linux-x64 -o ./app
```
> In case you are building for windows, change `linux-x64` to `win-x64`

## Configuration
Everything in AutoVersionIt is defined in a configuration file, and the application is designed to run from it without requiring additional parameters.

The configuration file (`autoversion.json`) should be located in the same directory where the executable is run.
Usually, this tool should be in the PATH, the configuration file should be in the root of your repository, and you should be
running it from the same root of your repository.

A sample configuration file is provided (`autoversion.sample.json`) to help you get started.

### Configuration quick grabs
To update your project's version information from the last tag in your repository by branch and commit the new tag as well:
- Save the following file as `autoversion.json` in the root of your repository:
```json
{
  "source": "git",
  "targets": [
    "git"
  ],
  "strategy": "simple",
  "patch": [
    "netcore"
  ],
  "bumpMethod": "revision"
}
```
- To use environment variables for the Development environment, use the following config file and save it as `autoversion.development.json` in the root of your repository:
```json
{
  "source": "env",
  "targets": [
    "env"
  ],
  "strategy": "rc",
  "patch": [
    "netcore"
  ],
  "bumpMethod": "suffix",
  "versionEnvFile": ".env",
  "versionEnv": "VERSION",
  "suffix": "rc"
}
```
  - The new version information will be written to the `.env` file in the root of your repository. Make sure to source this file into the environment of your CI/CD pipeline after the tool runs.
    ```bash
    source .env
    ```
- Run the tool as `./autoversionit` from the root of your repository from your `Development` branch, and it will increase the revision number of your version by one
  each time the tool is run.
- Set the environment variable `ENVIRONMENT` to `staging` and run the tool as usual to increase the build number of your version by one and reset the revision each time.
- Set the environment variable to `production` and run the tool again to increase the minor number and reset the build and revision.
- When doing a major release, manually tag the commit with the new version number on the development branch, and the tool will keep start all over from revision 0.
- Add your first tag and AutoVersionIt will take care of the rest.

### Alternatives
- You can also select how the Version will be incremented each time by changing the value of the `AUTOVERSIONIT_VERSION_BUMP_METHOD` environment variable to one of the following:
  - Major
  - Minor
  - Build
  - Revision
  - Or even set it to `none` to disable the version bumping altogether.
- If you want, you can also specify the increase method via command line parameters.
  - -major
  - -minor
  - -build
  - -revision
  - -nobump
- To set a custom-defined version number, you can set the `AUTOVERSIONIT_USE_VERSION` environment variable to the desired full version number and set the `AUTOVERSIONIT_VERSION_BUMP_METHOD` to `none`.

## Common Scenarios
- Reading version from Git tags and applying to project files. Commiting the new version back to the repository.
- Keeping version numbers consistent across multiple projects.
- Incrementing version numbers based on commit history.
- Sharing version information through environment variables.

# License
AutoVersionIt is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contributing
Contributions to AutoVersionIt are welcome! I'm open to any changes. Make a pull request and wait a few days for a review.
Here are some ways you can contribute:

- Bug reports and feature requests via issues
- Code contributions via pull requests
- Documentation improvements
- Testing and feedback

