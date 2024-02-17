# R-SBFL: Enhancing Robustness in Spectrum-Based Fault Localization
## Experimental Data
The subject suites studied in our work are collected at [here](https://www.dropbox.com/scl/fi/rgblrvo2h8ztlwsr39us5/Data.zip?rlkey=hgcnmz863fvhd4ecet9qn9y4n&dl=0), which contains Siemens, Unix utilities, Space and Defects4J. Each subject program has been packaged with several fault versions containing the spectrum of the passed and failed test cases and the location of faults. The details of these test suites are outlined in our paper titled "R-SBFL: Enhancing Robustness in Spectrum-Based Fault Localization." After downloading them, create a directory named _Data_ under the root directory to save these test suites.
## Experimental Program
Our experimental program comprises two essential components: a Python program for data preprocessing and test suite splitting and a C# program for sub-locators construction and integration. These two components seamlessly collaborate by partitioning their tasks and facilitating communication through socket connections, and they utilize SQL Server for large-scale data transmission. Regarding the SQL Server database files and log files constructed during the research, they have also been uploaded.
## C# Environment
First, install Microsoft Visual Studio 2017 and Microsoft SQL Server 2017, and also install the Microsoft SQL Server Management Studio.
### Microsoft SQL Server Management Studio:
1. Run Microsoft SQL Server Management Studio in administrator mode.
2. Navigate to "Databases" -> "Attach", then attach the copied database file (.mdf).
3. If unable to open the database diagram, use the "File" page in the "Database Properties" dialog box or the ALTER AUTHORIZATION statement to set the database owner to a valid login.
### Microsoft Visual Studio 2017:
1. Modify the paths in Program.cs to match your own data and result storage paths.
2. Run the program. If an exception is thrown stating "Database not open," navigate to the DataBase folder under the solution path (e.g., D:\Program\XXXXX\FrameWorkStatement\bin\Debug\DataBase\). Double-click on Connnection.udl, then change the server name (which can be found in Management Studio) to your local server name. Click on "Test Connection," and upon successful connection, click "OK." (If in release mode, locate the corresponding folder for modifications).
---
Note: This Readme assumes familiarity with Microsoft Visual Studio and Microsoft SQL Server Management Studio. If you encounter any difficulties during installation or usage, refer to the documentation of these tools or seek assistance from relevant forums or support channels.
## Python Environment
We use Conda to manage our environment. Please follow below steps to create the R-SBFL's Python environment.
  ```
  conda create -n RSBFL python=3.8
  conda activate RSBFL
  cd RSBFL_Python
  pip install -r requirements.txt
  conda deactivate
  ```
## Run
To run the C# program, double-click on the FrameWorkStatement.sln. When the Microsoft Visual Studio 2017 is open, 
change the "DirectoryInfo" _srcInfo_ to your data dictionary, and change the _dataDirectoryName_ to your result dictionary. Then, use Ctrl + F5 to run the solution. After the solution is running, use the following command to run the python code.
  ```
  python run_divider.py
  ```
