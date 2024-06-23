# Get tokens
The tokens, api id, secret, token and refresh token can be set in appsettings.json
If the user has no tokens, please get them:
- Follow the instructions in [this](https://www.reddit.com/wiki/api/) document to configure an application and get a secret
- Run the demo application and hit the Authorize button, enter the client id and the secret
- Select the read scope and authorize
- Copy the code presented in the response
	- Use postman to enter the parameters
	- In authorization use Basic Auth, set the username wuth the client id and the password with the secret
	- In headers, use the agent as found in appsettings.json
	- Use a form urlencoded body, the grant_type is "authorization_code", the code is the value retrieved in the demo app UI, the redirect url is the url set in the aplication setup in Reddit
	- The curl will look like this:

curl --location 'https://www.reddit.com/api/v1/access_token' \
--header 'User-Agent: postman:reditapidemo001:v1.0.0' \
--header 'content-type: application/x-www-form-urlencoded' \
--header 'Authorization: Basic xxxxxxxxx' \
--data-urlencode 'grant_type=authorization_code' \
--data-urlencode 'code=xxxxx' \
--data-urlencode 'redirect_uri=http://localhost:5095/redditauthentication'


- Take the tokens and set them in appsettings.json

## Arquitecture
This demo uses the Reddit.Net api package to manage the requests to the [Reddit API](https://github.com/sirkris/Reddit.NET), this package already takes care of rate limiting and provides strongly typed domain objects and monitoring events
The stats are collected in a separate process implemented as a hosted service and written to a concurrent dictionary which is later read in the controlers that provide the stats data
