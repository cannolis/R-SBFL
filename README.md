# R-SBFL
Enhancing Robustness in Spectrum-Based Fault Localization.
## Experimental Data
The subject suites studied in our work are collected at https://www.dropbox.com/scl/fi/rgblrvo2h8ztlwsr39us5/Data.zip?rlkey=hgcnmz863fvhd4ecet9qn9y4n&dl=0, which contains Siemens, Unix utilities, Space and Defects4J. Each subject program has been packaged with several fault versions containing the spectrum of the passed and failed test cases and the location of faults. The details of these test suites can be seen in our paper "R-SBFL: Enhancing Robustness in Spectrum-Based Fault Localization."
## Experimental Program
Our experimental program comprises two essential components: a Python program for data preprocessing and test suite splitting and a C# program for sub-locators construction and integration. These two components seamlessly collaborate by partitioning their tasks and facilitating communication through socket connections, and they utilize SQL Server for large-scale data transmission. Regarding the SQL Server database files and log files constructed during the research, they have also been uploaded.
