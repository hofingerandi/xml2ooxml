# xml2ooxml
_Disclaimer_: This program does not convert xml to ooxml, as the name implies.
Yet, it does something similar, and useful, so keep on reading ;-)

This program splits up a huge xml file into a folder structure, containing smaller (.xml/.txt) files.
The split-points and the resulting file names are defined via `XPath`s.
The intention is, to make versioning of the original XML-file easier (e.g. viewing file history, blaming lines, diffing, ...).

## Motivation
For storing PLC-configurations, a standardized XML file format is available, see https://en.wikipedia.org/wiki/PLCopen .
Unfortunately, the created XML files can be extremely large, and difficult to work with (e.g. viewing file history, blaming lines, diffing, ...)

## Goal 
In this project, I would like to provide a generic way to improve working with big xml files.
For the particular case of PLCOpenXML, the approach is as follows:
* store the project as plcopen-compatible xml
* on the build server, use this tool to create a comprehensible folder structure out of the xml file
* automatically commit the newly created files to some repo
* use the structure stored in the repo for viewing history, searching source code, ...

The whole process is built such, that in principle, the process is reversible, i.e., it is possible to modify the final, created xml-file, and convert it back to some huge file.
This could be beneficial, when e.g. some parts of the data should be created via (T4) code generation.

## Using the tool

### Command Line

There are just two command line options
```cmd
Xml2Ooxml.exe --createsample sampleconfig.xml
```
This create a sample configuration file, to be used in the conversion.
```cmd
Xml2Ooxml.exe --config myconfig.xml
```
Loads `myconfig.xml`, and starts the conversion as specified in this file.

### Configuration File

Below, I give an explanation for the various options.
Yet, I strongly recommend to just try the options with the provided samples.

#### DocumentPath
Path to the file that should be converted
#### TargetFolder
Folder for the resulting xml-file/folder structure
#### NamespaceEntries
Specify xml-namespaces that can later be used in the `XPath`-configuration

#### XPathsToExternalize
Every element that matches the `XPath` will be extracted to a separate xml-file.
#### MaxDepth
The extraction is done recursively for all elements, all children, all grandchildren, ... up to MaxDepth.
If the extraction fails for some `XPath`s, maybe `MaxDepth` was too low.

### Naming of files
A major challenge during the conversion, is the creation of stable names.
Small changes in the original xml-file should only cause small changes in the exported file/folder-structure.

Consider the following xml-snippet, where we externalize `Child`-elements:
```xml
<Parent>
  <Child name="Adam">...</Child>
  <Child name="Eve">..</Child>
</Parent>
```
By default, the files would be named `Parent/Child_Adam.xml` and `Parent/Child_Eve.xml`.

Several options are available, to customize this naming.


#### NameReplacement
Simple string-replacement. Useful, when `name`-Attributes contain annoying data (e.g. PLCOpenXml contains URL as name-tags).
#### LocalNameAsPrefix
By default, files are named `Child_Adam.xml`, and `Child_Eve.xml`.
When setting `LocalNameAsPrefix` to false, you would get `Adam.xml`, and `Eve.xml`.
#### XPath
You can create your very special hand crafted name via an XPath. Have an [XPath](https://docs.w3cub.com/xslt_xpath/) [cheat](https://devhints.io/xpath) [sheet](https://developer.mozilla.org/en-US/docs/Web/XPath) and some [XPath](https://www.utilities-online.info/xpath) [online](https://codebeautify.org/Xpath-Tester) [tester](http://xpather.com/) at hand.
Some examples of what can be achieved, are given in the unittests.

For the sample above, the XPath `concat(@name,',',name(),'-of-',name(..))` would yield
`Adam,Child-of-Parent.xml`, and `Eve,Child-of-Parent.xml`.

#### Other

Characters, that are not allowed on the filesystem, are automatically removed.

"Content-only"-files (i.e. no attributes, no further children), are stored as `.txt`-files; all others as `.xml` files.

## What's next

### Ooxml
Office Open Xml has a similar structure. A major difference, is that no direct links (paths) are stored within the parent document,
but only IDs. In a separate .rels-file, the mapping of these IDs to the actual files is stored.

I wouldn't expect big problems, when going into this direction, yet the benefits are also not clear.
Possibly, the resulting output gets slightly more unstable (if IDs are just increasing numbers, as is the case by default).

### Reversing the conversion

#### Namespaces

A technical issue that must be taken care of, when reverting the process are namespaces.
Currently, the links are just stored directly as an "ext"-attribute.
Of course, this might conflict with existing attributes.
So first, an additional namespace must be added, a unique prefix determined (again configurable...)
and the links added in the correct namespace.
After recombining the files, this namespace must be removed again as well.

Aside from that, I would not expect big problems in reverting the conversion.