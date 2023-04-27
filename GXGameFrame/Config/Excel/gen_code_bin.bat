set WORKSPACE=..\..

set GEN_CLIENT=%WORKSPACE%\Tools\LubanTools\Luban.ClientServer\Luban.ClientServer.exe

set CONF_ROOT=%WORKSPACE%\Config\Excel


%GEN_CLIENT% -j cfg --^
 -d %CONF_ROOT%\Defines\__root__.xml ^
 --input_data_dir %CONF_ROOT%\Datas ^
 --output_code_dir ..\..\Assets\GXGame\Config/Gen ^
 --output_data_dir ..\..\GenerateDatas\bytes ^
 --gen_types code_cs_unity_bin,data_bin ^
 -s all 

pause