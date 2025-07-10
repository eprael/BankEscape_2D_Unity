REM Remove all git history
rmdir /s /q .git

REM Initialize a new git repository
git init

REM Add all current files
git add .

REM Create initial commit
git commit -m "Initial commit"

REM Add your remote (replace with your actual repo URL)
git remote add origin https://github.com/eprael/BankEscape_2D_Unity.git

REM Push and set upstream to master branch, forcing overwrite
git push -u origin master --force