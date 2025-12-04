# After the jump 
A web-based choose-your-own adventure game with branching options

## How to run
### Running
Before staring the application run npm install
To run a production build run [npm run start], which goes into frontend and backend directory creating a compiled production build of the application and starts server on localhost:3000
To run server tests run [npm run test], which goes into test directory and runs the server tests.
For development server run [npm run dev] created an uncompiled node version of the application and runs the backend from the latest build compilation.

### Login
Normal user: has access to manage their own saved games. Aswell as account manager where they can manage their account. Here they can change their own username and password and delete their account.
Admin: has access to everything the normal user has as well as the admin terminal where they can edit and delete accounts. To access the admin panel the administrator presses the “admin” button, here they can press any of the registered accounts and change their username password or delete their account.
