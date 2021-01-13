The score.py is an example scoring script that showcases how to define sample input and output. This will be used by Azure ML to automatically generate the swagger for your API. An example swagger for the given scoring script is also included for your reference. 

The entry script file (score.py) and the conda dependencies file (myenv.yml) are both required when deploying a model (nyc.pkl) to an AKS cluster using Azure ML workspace. When you deploy a model using Azure ML with these 3 files, the swagger will be auto-generated.
