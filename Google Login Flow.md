First Time User Login:
Login.cshtml.cs (Creates Challenge) 
->
Middleware @ signin-google (Processes Response to Challenge)
->
HandleGoogleLogin.cshtml.cs (Handles login / registering of account, or redirect if the authentication failed in middleware)
->
WelcomeExternal (First time users will see this so they can see their temporary password as their account is created)
->
Index (Final redirect)

After First Time Login / Pre-existing account matching the email provided:
Login.cshtml.cs (Creates Challenge) 
->
Middleware @ signin-google (Processes Response to Challenge)
->
HandleGoogleLogin.cshtml.cs (Handles login / registering of account, or redirect if the authentication failed in middleware)
->
Index (Final redirect)