# forge-design.automation.autocad-test-harness

=============================
 
[![.net](https://img.shields.io/badge/.net-4.5-green.svg)](http://www.microsoft.com/en-us/download/details.aspx?id=30653)
[![odata](https://img.shields.io/badge/odata-4.0-yellow.svg)](http://www.odata.org/documentation/)
[![ver](https://img.shields.io/badge/Design%20Automation%20API-2.0-blue.svg)](https://developer.autodesk.com/en/docs/design-automation/v2)
 [![visual studio](https://img.shields.io/badge/Visual%20Studio-2012%7C2013%7C2015-green.svg)](https://www.visualstudio.com/)
[![License](http://img.shields.io/:license-mit-red.svg)](http://opensource.org/licenses/MIT)

##Description

This is a  harness of C# to test with Design Automation of AutoCAD for specific scenario: get Table info from a drawing

##Dependencies
* Visual Studio 2012. 2013 or 2015. The latest test is on VS2015.
* Get your credentials (client key and client secret) of Design Automation at http://developer.autodesk.com 
* [ObjectARX SDK] (http://usa.autodesk.com/adsk/servlet/index?siteID=123112&id=773204). The SDK version depends on which AutoCAD verison you want to test with the AppPackage locally. In current test, the version is AutoCAD 2016.
* put source DWG (with Tables) on a web driver such as S3 of AWS 

##Setup/Usage Instructions
* Firstly, test the workflow of package and workitem by Windows console program [Custom-Apppackage](CreateCloset.bundle/Custom-Apppackage)
  * open the solution [TrainingHarness](TrainingHarness.sln)
  * Unzip [ObjectARX SDK] (http://usa.autodesk.com/adsk/servlet/index?siteID=123112&id=773204). Add AcCoreMgd, AcDbMgd from SDK/inc to the project *PackageNetPlugin*
   * Restore the packages of project **Client** by [NuGet](https://www.nuget.org/). The simplest way is to right click the project>>"Manage NuGet Packages for Solution" >> "Restore" (top right of dialog)
  * Build project *PackageNetPlugin*. It is better to test with local AutoCAD to verify the custom command: load the .NET binary in AutoCAD, open the test DWG (with Tables), run the command 'MyPluginCommand', the code will dump all cell data of all Tables in the drawing and output to an json file: myTableData.json 
 
  * input your client key and client secret of Design Automation in  [Credentials.cs](./MyTestDesignAutomation/Credentials.cs).
  * input url of the DWG of web driver to  line 290 in [VariousInputs.cs](./MyTestDesignAutomation/VariousInputs.cs).
  * Build the solution and run the solution
  * Verify the whole process is working, and if a json file will be generated in MyDocuments folder. 
  
# License

This sample is licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT).
Please see the [LICENSE](LICENSE) file for full details.


## Written by

 [Xiaodong Liang](https://github.com/xiaodongliang/) [Forge Partner Development](http://forge.autodesk.com)

