# xml2ooxml
Split up a huge xml file into a folder structure, containing smaller (xml) files.
The split-points are defined via `XPath`s.
This can make versioning of the original XML-file easier (e.g. viewing file history, blaming lines, diffing, ...).

## Motivation
For storing PLC-configurations, a standardized XML file format is available, see https://en.wikipedia.org/wiki/PLCopen
Unfortunately, the created XML files can be extremely large, and difficult to work with (e.g. viewing file history, blaming lines, diffing, ...)

## Goal 
In this project, I would like to provide a generic way to improve working with big xml files.
For the particular case of PLCOpenXML, the approach is
* store the project as plcopen-compatible xml
* on the build server, use this tool to create a comprehensible folder structure out of the xml file
* automatically commit the newly created files to the repo

The whole process is built such, that in principle, the process is reversible, i.e., it is possible to modify the final, created xml-file, and convert it back to some huge file.
This is beneficial, when e.g. some parts of the data should be created via (T4) code generation.
