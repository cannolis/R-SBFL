# R-SBFL: Enhancing Robustness in Spectrum-Based Fault Localization
## Experimental Data
The subject suites studied in our work are collected at [here](https://www.dropbox.com/scl/fi/rgblrvo2h8ztlwsr39us5/Data.zip?rlkey=hgcnmz863fvhd4ecet9qn9y4n&dl=0), which contains Siemens, Unix utilities, Space and Defects4J[1,2,3,4]. Each subject program has been packaged with several fault versions containing the spectrum of the passed and failed test cases and the location of faults. The details of these test suites are outlined in our paper titled "R-SBFL: Enhancing Robustness in Spectrum-Based Fault Localization." After downloading the .zip file, unzip it in a directory to save these test suites.

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

## Reference
1. Hyunsook Do, Sebastian Elbaum, and Gregg Rothermel. 2005. Supporting Controlled Experimentation with Testing Techniques: An Infrastructure and Its Potential Impact. Empirical Software Engineering 10 (2005), 405–435. https://doi.org/10.1007/s10664-005-3861-2
2. Ross Gore and Paul Reynolds. 2012. Reducing Confounding Bias in Predicate-Level Statistical Debugging Metrics. In 2012 34th International Conference on Software Engineering. 463–473. https://doi.org/10.1109/ICSE.2012.6227169
3. René Just, Darioush Jalali, and Michael D. Ernst. 2014. Defects4J: A Database of Existing Faults to Enable Controlled Testing Studies for Java Programs. In Proceedings of the 2014 International Symposium on Software Testing and Analysis. Association for Computing Machinery, 437–440. https://doi.org/10.1145/2610384.2628055
4. Kai Yu, Mengxiang Lin, Qing Gao, Hui Zhang, and Xiangyu Zhang. 2011. Locating Faults Using Multiple Spectra-Specific Models. In Proceedings of the 2011 ACM Symposium on Applied Computing. Association for Computing Machinery, 1404–1410. https://doi.org/10.1145/1982185.1982490

