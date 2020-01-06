# PI Agent SOW
  
## Disclaimer
  
This README will be completed in the following days

## Configuration

We added two parameters:

1. E_BASE_URL (base url for Asset Hub api) 
2. PI_ATTRIBUTE_DEFINITION (a list of comma separated attribute names, used to retrive points info from a PI data archive)

Example:
"name,pointsource,description,digitalset,engunits,exdesc,future,pointtype,ptclassname,sourcetag,archiving,compressing,span,step,zero,changedate,changer,creationdate,creator,pointid,instrumentag"

## PI_AGENT_CONFIG_FILE environment parameter

There are two different agent configuration files shipped with the projects (agent and test projects). Both have the same name (agentconfiguration.json).
We have included theses files only to make tests easier. They are not really necessary, as the general idea is to retrieve the file location from the environment variable (at least in the file based configuration scenario). 

The PI_AGENT_CONFIG_FILE parameter can accept an absolute path or a relative path (i.e., just the filename, without the path). In case of a relative path, the file must be placed at the output path of the project, along with the executable and dlls. The existing files are configured to be deployed that way.

Important:
The unit tests sets the environment variables. Invalid parameter values can break the agent.

## Architecture

This is a console application. To mimic a windows service, i.e., to stay loaded in memory, it waits for an input to exit (control-c), provided all mandatory parameters are provided. If they are not, it just writes a message to console ("Hello world"), in order to fullfill the Circle CI build test, and exits.



