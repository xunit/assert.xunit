# Contributing

## Code of Conduct

Please note that this project is released with a [Contributor Code of Conduct](http://www.dotnetfoundation.org/code-of-conduct). By participating in this project you agree to abide by its terms.

## Reporting Issues

### Before you file a bug...

* Is this a question, or are you looking for help? Ask it in our [discussions area](https://github.com/xunit/xunit/discussions/) instead.
* Did you [read the documentation](https://xunit.net/)?
* Did you search the issues list to see if someone already reported it?
* Did you create a simple repro for the problem?

### Before you submit a PR...

* Did you ensure this is an [accepted 'help wanted' issue](https://github.com/xunit/xunit/issues?q=is%3Aopen+is%3Aissue+label%3A%22help+wanted%22)? (If not, open one to start the discussion)
* Did you read the [project governance](https://xunit.net/governance)?
* Does the code follow existing coding styles? (tabs, comments, no regions, etc.)?
* Did you write unit tests?

## Contributing Changes

We share the assertion library between the `main` branch (v3) and the `v2` branch of `xunit/xunit`. This includes the tests. The workflow below suggests the best way to ensure you are writing code that will work for both v2 and v3, as the build tools in the `main` branch enforce all the necessary rules. _If you attempt to issue a PR against the `v2` branch, we will ask you to redo the changes against `main` before accepting them._

### Suggested Workflow

1. Pick an existing issue or create a new one.
2. Checkout the `main` branch in the [xunit/xunit](https://github.com/xunit/xunit) repository.
3. Initialize or update the `Asserts` submodule.
4. Change the origin of the `Asserts` submodule to your personal fork.
5. Create a branch off of the `main` branch in the `Asserts` submodule.
6. Implement your changes using the [xunit/xunit/xunit.sln](https://github.com/xunit/xunit/blob/main/xunit.sln) solution.
7. Verify the implementation by running [xunit/xunit/build.ps1](https://github.com/xunit/xunit/blob/main/build.ps1) on Windows or [xunit/xunit/build](https://github.com/xunit/xunit/blob/main/build) on Linux.
8. Push your `Asserts` submodule branch to your fork.
9. Submit a PR against this repositories `main` branch.
10. A maintainer will review and merge your PR then update the submodule pointer of the `xunit/xunit` repository.

It is recommended to also add tests to verify the changes.
These additional steps are required:

1. Create a branch off of the `main` branch in your personal fork of the `xunit/xunit` repository.
2. Implement the tests.
3. Verify the implementation by running the build script as outlined in step 7 of the primary workflow.
4. Push your branch to your fork.
5. Submit a PR against the `xunit/xunit` repositories `main` branch.
6. A maintainer will review and merge your PR then merge the changes into any older branches still actively maintained.
