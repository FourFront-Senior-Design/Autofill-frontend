# Database Auto-fill Software GUI

[![Build Status](https://dev.azure.com/ShashwatiShradha/Database%20Autofill%20Software/_apis/build/status/FourFront-Senior-Design.categorization?branchName=development)](https://dev.azure.com/ShashwatiShradha/Database%20Autofill%20Software/_build/latest?definitionId=2&branchName=development) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/FourFront-Senior-Design/AutofillFrontend/blob/master/Copyright)

The Database Auto-full Software GUI projects is the frontend of the auto-fill project. It asks the user the name of the section that needs to be auto-filled, and on-clicking the auto-fill button, it runs the backend scripts to fill the MS Access Database. It updates the user of the current step the scripts are on using a progress bar. Open the MS Access database or use the [Data Entry Form](https://github.com/FourFront-Senior-Design/DataEntryForm) to verify that the section is filled.

### Prerequisites
* [Microsoft Access Database Manager 2010 (x64 bit)](https://www.microsoft.com/en-US/download/details.aspx?id=13255)

### Setup
* Install [Microsoft Visual Studios 2017](https://docs.microsoft.com/en-us/visualstudio/productinfo/2017-redistribution-vs) (or higher) with the [.NET framework 4.7.2](https://docs.microsoft.com/en-us/dotnet/framework/install/guide-for-developers).
* Setup the python backend environment as directed [here](https://github.com/FourFront-Senior-Design/pythonenv/blob/master/README.md).
* Build and run the project using Visual Studios. Make sure all the projects are running on the x64 platform.
* Test and run the project on the [Test Databases](https://github.com/FourFront-Senior-Design/AutofillFrontend/tree/master/TestDatabases) sections. **Note**: The path should be to the section folder and *not* to the access database directly.

### Features
* Autofilling the database
* Progress bar
* Logging
* Error handling

### Development
* The project uses [MVVM design Pattern](https://docs.microsoft.com/en-us/archive/msdn-magazine/2009/february/patterns-wpf-apps-with-the-model-view-viewmodel-design-pattern).
* Contributors and future developers are requested to [fork](https://guides.github.com/activities/forking/) from this project.
