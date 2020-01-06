MSBuild .\OsisoftPIAgentSOW.sln  -t:Rebuild -p:Configuration=Release -p:outputdir=.\deploy\

docker build -t radixeng\osisoft-pi-agent-sow:v1 .

docker run --name osisoft-pi-agent-sow-container osisoft-pi-agent-sow-image

