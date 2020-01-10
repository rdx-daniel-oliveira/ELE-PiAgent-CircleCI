#MSBuild .\OsisoftPIAgentSOW.sln  -t:Rebuild -p:Configuration=Release -p:outputdir=.\deploy\

docker build -t radixeng/osisoft-pi-agent-sow:v1 .

docker run --rm -it radixeng/osisoft-pi-agent-sow:v1

