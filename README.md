# R-SBFL: Enhancing Robustness in Spectrum-Based Fault Localization
## Experimental Data
The subject suites studied in our work are collected at [here](https://www.dropbox.com/scl/fi/rgblrvo2h8ztlwsr39us5/Data.zip?rlkey=hgcnmz863fvhd4ecet9qn9y4n&dl=0), which contains Siemens, Unix utilities, Space and Defects4J. Each subject program has been packaged with several fault versions containing the spectrum of the passed and failed test cases and the location of faults. The details of these test suites are outlined in our paper titled "R-SBFL: Enhancing Robustness in Spectrum-Based Fault Localization." After downloading the .zip file, unzip it in a directory to save these test suites.

## Experimental Program
Our experimental program comprises two essential components: a Python program named _RSBFL_Python_for data preprocessing and test suite splitting and a C# program named _RSBFL_C#_for sub-locators construction and integration. These two components seamlessly collaborate by partitioning their tasks and facilitating communication through socket connections, and they utilize SQL Server for large-scale data transmission. Regarding the SQL Server database files and log files constructed before the research, they have also been uploaded.

## C# Environment
First, install Microsoft Visual Studio 2017 and Microsoft SQL Server 2017, and also install the Microsoft SQL Server Management Studio. Then proceed with the following settings.
### Microsoft SQL Server Management Studio:
1. Run Microsoft SQL Server Management Studio in administrator mode.
2. Navigate to "Databases" -> "Attach", then attach the uploaded database file (.mdf).
3. If unable to open the database diagram, use the "File" page in the "Database Properties" dialog box or the ALTER AUTHORIZATION statement to set the database owner to a valid login.
### Microsoft Visual Studio 2017:
1. Navigate to the _DataBase_ folder located within the solution path, for example, "/R-SBFL/RSBFL_C#/FrameWorkStatement/bin/Debug/DataBase/" and "/R-SBFL/RSBFL_C#/FrameWorkStatement/bin/Release/DataBase/". Double-click on "Connection.udl", then update the server name (as identified in Microsoft SQL Server Management Studio) to your local server's name. Click on "Test Connection" and, following a successful connection, click "OK".

## Python Environment
We use Conda to manage our environment. Please follow the steps below to create the R-SBFL's Python environment.
  ```
  conda create -n RSBFL python=3.8
  conda activate RSBFL
  cd RSBFL_Python
  pip install -r requirements.txt
  conda deactivate
  ```

## Run
For the Python program, modify the parameters (_host_, _user_name_, _password_, and _database_name_) within the _database_args_ in "/RSBFL_Python/run_divider.py" to match your server's credentials. Afterward, execute the Python program using the command provided below.
  ```
  conda activate RSBFL
  cd RSBFL_Python
  python run_divider.py
  ```
After the Python program is running, follow the steps below to run the C# program so that it can establish communication with the Python program. 
1. Open the "RSBFL_C#" folder.
2. Double-click on the "FrameWorkStatement.sln" file to open the solution. Once opened, update the _srcInfo_ variable in "Program.cs" to point to your data directory. You can download the necessary data from [here](https://www.dropbox.com/scl/fi/rgblrvo2h8ztlwsr39us5/Data.zip?rlkey=hgcnmz863fvhd4ecet9qn9y4n&dl=0), and remember to unzip the file after downloading. Additionally, adjust the _dataDirectoryName_ variable in "Program.cs" to reference your results directory.
3. Press "Ctrl + F5" to execute the solution, which will display the console's output. Then, press any key to initiate the full program execution. Upon completion, the experimental outcomes will be saved as Excel files in the designated output folder.

-----------------------------------
Note: This readme assumes familiarity with Microsoft Visual Studio and Microsoft SQL Server Management Studio. If you encounter any difficulties during installation or usage, refer to the documentation of these tools or seek assistance from relevant forums or support channels.


