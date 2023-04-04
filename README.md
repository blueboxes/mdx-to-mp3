# mdx-to-mp3

This is a simple console application that takes an MDX file and outputs a MP3. It is here as an example and I will happily accept pull requests.

A [blog post](http://www.blueboxes.co.uk/enhancing-accessibility-building-a-read-this-page-feature-with-azure-speech-service-and-c) can be found at detailing it's full usage.

## Usage 
This program accepts two command line args, one for the source mdx file and the other for the target mp3 file.

Before running make sure you rename  `sample.appsettings.json` to `appsettings.json` file and complete the details

## Running locally
This console app is build with C#7 and can be run with by providing an markdown file location and the target mp3 file path.

From the `src` folder run the following:

```
dotnet run ../sample.mdx ../sample.mp3
```