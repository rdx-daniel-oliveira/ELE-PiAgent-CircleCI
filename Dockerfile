#Base image= Windows server core 2016 containing .NET Framework 4.8
FROM mcr.microsoft.com/dotnet/framework/runtime:4.8-windowsservercore-ltsc2016
SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop';"]

#Add Directories containing the files that will be used for PI Agent installation and runtime
ADD AFClient_2.10.7.283 C:/AFClient_2.10.7.283/
ADD deploy C:/deploy/

# Run AF SDK Installer. "AFClient_2.10.7.283\silent.ini" file has been changed to install only AFSDK
RUN Start-Process ".\\AFClient_2.10.7.283\\Setup.exe" -ArgumentList "/f","AFClient_2.10.7.283\\silent.ini" -NoNewWindow -Wait 

# Set Environment Variables for PI Agent work properly
RUN "[Environment]::SetEnvironmentVariable('PATH',$env:PATH + ';C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319',[EnvironmentVariableTarget]::Machine)"
RUN "[Environment]::SetEnvironmentVariable('PI_DATA_ARCHIVE_NAME','radixuspisandbox-centralpiserver.southcentralus.cloudapp.azure.com',[EnvironmentVariableTarget]::Machine)"
RUN "[Environment]::SetEnvironmentVariable('PI_USER','rdxpisandbox\\sergio.treiger',[EnvironmentVariableTarget]::Machine)"
RUN "[Environment]::SetEnvironmentVariable('PI_PASSWORD','MooseT4nk',[EnvironmentVariableTarget]::Machine)"
RUN "[Environment]::SetEnvironmentVariable('E_USER','ben.perez@radixeng.com',[EnvironmentVariableTarget]::Machine)"
RUN "[Environment]::SetEnvironmentVariable('E_PASSWORD','R@dix.Rocks19',[EnvironmentVariableTarget]::Machine)"
RUN "[Environment]::SetEnvironmentVariable('E_ORG_ID','1',[EnvironmentVariableTarget]::Machine)"
RUN "[Environment]::SetEnvironmentVariable('E_DATASET_ID','1',[EnvironmentVariableTarget]::Machine)"
RUN "[Environment]::SetEnvironmentVariable('E_AGENT_INTERVAL_MIN','1',[EnvironmentVariableTarget]::Machine)"
RUN "[Environment]::SetEnvironmentVariable('E_BASE_URL','https://evergreening.ean.io/datasets/v1/',[EnvironmentVariableTarget]::Machine)"
RUN "[Environment]::SetEnvironmentVariable('PI_ATTRIBUTE_DEFINITION','name,pointsource,description,digitalset,engunits,exdesc,future,pointtype,ptclassname,sourcetag,archiving,compressing,span,step,zero,changedate,changer,creationdate,creator,pointid,instrumentag',[EnvironmentVariableTarget]::Machine)"
RUN "[Environment]::SetEnvironmentVariable('E_STATIC_TOKEN','bold-restaurant-motor',[EnvironmentVariableTarget]::Machine)"


# Run PI Agent as a console application
ENTRYPOINT ["deploy\\OSIsoftPIAgentSOW.exe"]