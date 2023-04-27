set WORKSPACE=..\..
set GEN_CLIENT=%WORKSPACE%\Tools\LubanTools\Luban.ClientServer\Luban.ClientServer.exe

set CONF_ROOT=%WORKSPACE%\Config\Excel

%GEN_CLIENT% -j cfg --generateonly --^
 -d %CONF_ROOT%\Defines\__root__.xml ^
 --input_data_dir %CONF_ROOT%\Datas ^
 --output_data_dir dummy ^
 --gen_types data_json ^
 -s all
pause