<p align="center">
<img
    src="https://avatars2.githubusercontent.com/u/34855698?s=460&v=4" width="250px">
</p>

<p align="center">      
  <a href="https://travis-ci.org/camchain/cam-server">
    <img src="https://travis-ci.org/camchain/cam-server.svg?branch=master">
  </a>
  <a href="https://github.com/camchain/cam-server/blob/master/LICENSE">
    <img src="https://img.shields.io/badge/license-MIT-blue.svg">
  </a>
  <a href="https://github.com/camchain/cam-server/releases">
    <img src="https://badge.fury.io/gh/camchain%2Fcam-server.svg" alt="Current cam-server version.">
  </a>  
</p>


Supported Platforms
--------

We already support the following platforms:

* CentOS 7
* Docker
* macOS 10 +
* Red Hat Enterprise Linux 7.0 +
* Ubuntu 14.04, Ubuntu 14.10, Ubuntu 15.04, Ubuntu 15.10, Ubuntu 16.04, Ubuntu 16.10
* Windows 7 SP1 +, Windows Server 2008 R2 +

We will support the following platforms in the future:

* Debian
* Fedora
* FreeBSD
* Linux Mint
* OpenSUSE
* Oracle Linux

Development
--------

To start building peer applications for CAM on Windows, you need to download [Visual Studio 2017](https://www.visualstudio.com/products/visual-studio-community-vs), install the [.NET Framework 4.7 Developer Pack](https://www.microsoft.com/en-us/download/details.aspx?id=55168) and the [.NET Core SDK](https://www.microsoft.com/net/core).

If you need to develop on Linux or macOS, just install the [.NET Core SDK](https://www.microsoft.com/net/core).

To install CAM SDK to your project, run the following command in the [Package Manager Console](https://docs.nuget.org/ndocs/tools/package-manager-console):

```
PM> Install-Package CAM
```

For more information about how to build DAPPs for CAM, please read the [documentation](http://docs.camatrix.org/en-us/sc/introduction.html)|[文档](http://docs.camatrix.org/zh-cn/sc/introduction.html).

Daily builds
--------

If you want to use the [latest daily build](https://www.myget.org/feed/cam/package/nuget/CAM) then you need to add a NuGet.Config to your app with the following content:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <packageSources>
        <clear />
        <add key="MyGet-CAM" value="https://www.myget.org/F/cam/api/v3/index.json" />
        <add key="NuGet.org" value="https://api.nuget.org/v3/index.json" />
    </packageSources>
</configuration>
```

*NOTE: This NuGet.Config should be with your application unless you want nightly packages to potentially start being restored for other apps on the machine.*

How to Contribute
--------

You can contribute to CAM with [issues](https://github.com/camchain/CAM/issues) and [PRs](https://github.com/camchain/CAM/pulls). Simply filing issues for problems you encounter is a great way to contribute. Contributing implementations is greatly appreciated.

We use and recommend the following workflow:

1. Create an issue for your work.
    * You can skip this step for trivial changes.
	* Reuse an existing issue on the topic, if there is one.
	* Clearly state that you are going to take on implementing it, if that's the case. You can request that the issue be assigned to you. Note: The issue filer and the implementer don't have to be the same person.
1. Create a personal fork of the repository on GitHub (if you don't already have one).
1. Create a branch off of master(`git checkout -b mybranch`).
    * Name the branch so that it clearly communicates your intentions, such as issue-123 or githubhandle-issue.
	* Branches are useful since they isolate your changes from incoming changes from upstream. They also enable you to create multiple PRs from the same fork.
1. Make and commit your changes.
1. Add new tests corresponding to your change, if applicable.
1. Build the repository with your changes.
    * Make sure that the builds are clean.
	* Make sure that the tests are all passing, including your new tests.
1. Create a pull request (PR) against the upstream repository's master branch.
    * Push your changes to your fork on GitHub.

Note: It is OK for your PR to include a large number of commits. Once your change is accepted, you will be asked to squash your commits into one or some appropriately small number of commits before your PR is merged.

License
------

The CAM project is licensed under the [MIT license](LICENSE).
