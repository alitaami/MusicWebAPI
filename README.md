<h1 align="center">🎵 Music Web API</h1>
<p align="center">
  A robust and scalable containerized <strong>.NET Web API</strong> inspired by <strong>Spotify</strong>, designed to deliver high performance and seamless user experience. This project incorporates advanced features including <strong>Hangfire</strong> for background job scheduling, <strong>JWT-based authentication</strong> for secure access control, <strong>Swagger</strong> for comprehensive API documentation, <strong>SignalR</strong> to enable real-time chat functionality, <strong>Kibana</strong> for centralized log monitoring, <strong>Minimal APIs</strong> for streamlined endpoints, <strong>MinIO</strong> as an object storage solution, and <strong>Redis</strong> for efficient caching and improved responsiveness.
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-blue" />
  <img src="https://img.shields.io/badge/Docker-Compose-blue" />
  <img src="https://img.shields.io/badge/Architecture-Clean-green" />
  <img src="https://img.shields.io/badge/Auth-JWT-orange" />
</p>

<h2>🚀 Project Features</h2>
<ul>
  <li>✅ JWT Authentication with role-based access</li>
  <li>✅ Google OAuth 2.0 </li>
  <li>✅ Hangfire Dashboard for background job monitoring (accessible only to users with the <strong>SuperUser</strong> role)</li>
  <li>✅ Swagger UI for easy API testing</li>
  <li>✅ Real-time chat with SignalR</li>
  <li>✅ ElasticSearch + Kibana integration for log monitoring</li>
  <li>✅ Redis caching for fast data access</li>
  <li>✅ MinIO object storage integration</li>
  <li>✅ Minimal APIs support in .NET 8</li>
  <li>✅ Clean, maintainable architecture (Clean Architecture)</li>
  <li>✅ Containerized with Docker and Docker Compose</li>
  <li>✅ Subscription plans with Stripe payment gateway integration</li>
</ul>

<h2>🛠️ Technologies Used</h2>
<table>
  <tr><th>Stack</th><th>Details</th></tr>
  <tr><td><strong>Backend</strong></td><td>ASP.NET Core Web API (.NET 8, Minimal APIs)</td></tr>
  <tr><td><strong>Real-Time</strong></td><td>SignalR</td></tr>
  <tr><td><strong>Background Jobs</strong></td><td>Hangfire</td></tr>
  <tr><td><strong>Logging</strong></td><td>Serilog + Elasticsearch + Kibana</td></tr>
  <tr><td><strong>Auth</strong></td><td>JWT Bearer Token</td></tr>
  <tr><td><strong>Docs</strong></td><td>Swagger / OpenAPI</td></tr>
  <tr><td><strong>Object Storage</strong></td><td>MinIO – store, download and stream uploaded songs securely</td></tr>
  <tr><td><strong>Caching</strong></td><td>Redis</td></tr>
  <tr><td><strong>Containerization</strong></td><td>Docker, Docker Compose</td></tr>
  <tr><td><strong>Database</strong></td><td>PostgreSQL</td></tr>
</table>

<h2>📦 Getting Started</h2>

<h3>🔧 Prerequisites</h3>
<ul>
  <li><a href="https://dotnet.microsoft.com/download">.NET SDK</a></li>
  <li><a href="https://www.docker.com/">Docker</a></li>
  <li><a href="https://docs.docker.com/compose/">Docker Compose</a></li>
</ul>

<h2>📄 Environment (.env) & AppSettings (appsettings.json) Files</h2>
<p>
  The <code>.env</code> & <code>appsettings.json</code> files are not included in the repository. Download them from the following Google Drive links:
</p>
<p>
  <a href="https://drive.google.com/file/d/1PISHbiUgCH54-B5mt7jEKzO2ubvsgrvK/view?usp=sharing" target="_blank">
    👉 .env file
  </a>
</p>
<p>
  <a href="https://drive.google.com/file/d/1iysuL5EzC4rUrbHnlBEka4qJZUSlayZE/view?usp=drive_link" target="_blank">
    👉 appsettings.json file
  </a>
</p>
<p>
  After downloading, place the <code>.env</code> & <code>appsettings.json</code> files in the root of the <code>MusicWebApi.API</code> layer.
</p>

<h2>🐳 Running the Project</h2>

<h3>First-Time Setup or After Major DB/Data Changes:</h3>
<pre><code>docker-compose down -v --remove-orphans
docker network prune -f
docker-compose up --build -d
</code></pre>

<h3>After Small Changes (e.g., code updates):</h3>
<pre><code>docker-compose up -d --build
</code></pre>

<h2>📂 Project Structure (Clean Architecture)</h2>
<pre><code>/tests
  ├── MusicWebAPI.UnitTests            → Unit Tests
/src
  ├── MusicWebAPI.API                  → Web entry point
  ├── MusicWebAPI.Application          → Business logic and use cases
  ├── MusicWebAPI.Domain               → Core domain models/entities
  ├── MusicWebAPI.Infrastructure       → EF Core, external services, logging, Redis, MinIO
  └── MusicWebAPI.Core                 → Shared constants, utilities, resources
</code></pre>

<h2>🔐 Authentication</h2>
<p>JWT Bearer Authentication is enabled. Use your token in Swagger via the <code>Authorize</code> button.</p>
<p>Roles (<code>SuperUser</code>, <code>Artist</code>, <code>User</code>) are embedded in the JWT claims.</p>

<h3>🧪 Test Accounts</h3>
<ul>
  <li><strong>SuperUser</strong>
    <pre><code>{
  "email": "SuperUser@gmail.com",
  "password": "superUser123"
}</code></pre>
  </li>
  <li><strong>User</strong>
    <pre><code>{
  "email": "alitaami@gmail.com",
  "password": "19851381"
}</code></pre>
  </li>
  <li><strong>Artist</strong>
    <pre><code>{
  "email": "dariush@gmail.com",
  "password": "19851381"
}</code></pre>
  </li>
</ul>

<h2>🛢️ Database Access</h2>
<p>To connect locally using <a href="https://www.beekeeperstudio.io/">Beekeeper Studio</a> or any PostgreSQL client, use:</p>
<ul>
  <li><strong>Connection Type:</strong> PostgreSQL</li>
  <li><strong>Host:</strong> localhost</li>
  <li><strong>Port:</strong> 5432</li>
  <li><strong>User:</strong> sa</li>
  <li><strong>Password:</strong> sa123</li>
  <li><strong>Database:</strong> MusicDb</li>
</ul>
<img src="https://github.com/user-attachments/assets/d682a25f-df28-48c1-bfc4-d94817662ae5" alt="Beekeeper Screenshot" />


<h2>🧠 ML-Based Song Recommendation</h2> <p> Leveraging <strong>Spotify's API</strong> alongside user interaction data stored in the local database, the project uses <strong>ML.NET</strong> to train a recommendation engine that suggests songs tailored to each user’s preferences. </p> <ul> <li>✅ Combines Spotify metadata with local listening behavior</li> <li>✅ ML model trained periodically using <code>Hangfire</code></li> <li>✅ Trained recommendation results are cached in <code>Redis</code> for quick access</li> <li>✅ Supports extensible training pipeline with ML.NET</li> </ul>


<h2>💳 Stripe Payment Gateway for Subscription Plans</h2>
<p>
  This project integrates <strong>Stripe</strong> as the payment gateway to manage subscription plans securely and efficiently.
</p>

<h3>How Stripe Works Here:</h3>
<ul>
  <li>When a user subscribes to a plan, the backend generates a <strong>Stripe Checkout session URL</strong>.</li>
  <li>The frontend or API consumer is redirected to this secure Stripe-hosted payment page.</li>
  <li>Stripe handles all sensitive card data and payment processing.</li>
  <li>After successful payment, Stripe redirects back to your app with confirmation.</li>
  <li>Webhooks can be configured (optional) to update subscription status in the system.</li>
</ul>

<h3>Testing Stripe Integration</h3>
<p>Use the following <strong>test card details</strong> when testing the subscription payment flow with Stripe in development mode:</p>
<table border="1" cellpadding="8" cellspacing="0" style="border-collapse: collapse;">
  <thead>
    <tr>
      <th>Field</th>
      <th>Value</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>Card Number</td>
      <td><code>4242 4242 4242 4242</code></td>
    </tr>
    <tr>
      <td>Expiry Date</td>
      <td><code>12/29</code></td>
    </tr>
    <tr>
      <td>CVC / CVV</td>
      <td><code>123</code></td>
    </tr>
    <tr>
      <td>ZIP Code</td>
      <td>Any 5-digit number</td>
    </tr>
  </tbody>
</table>

<p>
  These are Stripe's official test card numbers — no real money will be charged.<br/>
  Use these in the Stripe Checkout page during subscription testing.<br/>
  Stripe will simulate successful or failed payments based on the card number used.
</p>

<h3>Summary</h3>
<p>
  With Stripe integrated:<br/>
  - Your API generates secure, single-use Checkout URLs for each subscription.<br/>
  - The subscription workflow is seamless and PCI-compliant.<br/>
  - Test payments are easy with Stripe’s official test cards.<br/>
  - This ensures users can safely subscribe to your music plans, with billing handled by Stripe.
</p>
<img src="https://github.com/user-attachments/assets/52ec43d0-bf21-4d2a-b740-320bb9fe523a" alt="Beekeeper Screenshot" />


<h2>🎧 Limit Streaming Feature</h2>
<p>
  To manage system resources and enforce fair usage policies, the API includes a <strong>Limit Streaming</strong> feature that restricts how many times a user can stream a specific song within a certain timeframe.
</p>

<h3>🧠 Logic Overview</h3>
<ul>
  <li>Each time a user streams a song, the system logs the attempt in the Redis cache.</li>
  <li>The stream count is tracked using a unique key per user and song combination (e.g., <code>ListenCount:{userId}:{yyyyMMdd}:{windowGroup}</code>).</li>
  <li>If the user exceeds the allowed number of streams (e.g., <strong>10 streams per 12 hour</strong> for non-premium users), the API blocks further attempts with an appropriate error response </li>
  <li>Premium users may have higher or unlimited stream limits based on their subscription tier.</li>
</ul>

<h3>🚫 Error Response Example</h3>
<pre><code>
{
  "data": null,
  "errorMessage": "Stream limit reached. Please wait for your 12-hour window to reset or consider subscribing to our premium plan for unlimited access.",
  "isSuccess": false,
  "statusCode": 422,
  "errors": []
}</code></pre>

<p>
  This mechanism ensures fair usage and protects system performance while still offering flexibility for premium subscribers.
</p>


<section>
  <h2>🛠 Dev Tools & Dashboards</h2>
  <p>📘 SWAGGER / 🕒 HANGFIRE / KIBANA (Minor Enhancements)</p>
  <table border="1" cellpadding="8" cellspacing="0" style="border-collapse: collapse; width: 100%;">
    <thead>
      <tr>
        <th>Tool</th>
        <th>URL</th>
        <th>Description</th>
        <th>Screenshot</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td>Swagger</td>
        <td><a href="http://localhost:8080/swagger/index.html" target="_blank" rel="noopener noreferrer">http://localhost:8080/swagger/index.html</a></td>
        <td>API documentation &amp; testing</td>
        <td>
              <img src="https://github.com/user-attachments/assets/d6a559b3-89aa-4978-a342-cc1a54cb49cf"
                alt="Chat Screenshot 2" 
                style="max-width: 150px; height: auto;" /> 
        </td>
      </tr>
      <tr>
  <td>Chat UI</td>
  <td>
    <a href="http://localhost:8080/chat" target="_blank" rel="noopener noreferrer">
      http://localhost:8080/chat
    </a>
  </td>
  <td>Real-time chat playground including login/register </td>

  <td style="display: flex; gap: 10px;">
    <img src="https://github.com/user-attachments/assets/1889b7db-ef42-4868-bb45-6fe2d5e8fd9b" 
         alt="Chat Screenshot 2" 
         style="max-width: 150px; height: auto;" />
    <img src="https://github.com/user-attachments/assets/1c12d252-4525-4964-8c53-fbce2bb6a337" 
         alt="Chat Screenshot 3" 
         style="max-width: 150px; height: auto;" />
    <img src="https://github.com/user-attachments/assets/cc8ee4f7-b919-430e-b9c2-52fcd50b1e6a" 
         alt="Real-Time Chat Screenshot" 
         style="max-width: 150px; height: auto;" />      
  </td>
</tr>
      <tr>
        <td>Hangfire</td>
        <td><a href="http://localhost:8080/hangfire" target="_blank" rel="noopener noreferrer">http://localhost:8080/hangfire</a></td>
        <td>Job scheduling &amp; dashboard</td>
        <td>
          <img src="https://github.com/user-attachments/assets/81504572-a251-4f40-8782-24c2b7451d30" alt="Hangfire Screenshot" style="max-width: 150px; height: auto;" />
        </td>
      </tr>
      <tr>
        <td>Kibana</td>
        <td><a href="http://localhost:5601" target="_blank" rel="noopener noreferrer">http://localhost:5601</a></td>
        <td>Log visualization via Serilog</td>
        <td>
          <img src="https://github.com/user-attachments/assets/be466f44-c7ae-4365-b40e-486f3bc244bf" alt="Kibana Monitoring" style="max-width: 150px; height: auto;" />
        </td>
      </tr>
    </tbody>
  </table>
</section>


<h2>🧪 Testing</h2>
<ul>
  <li>Unit Tests: <code>NUnit</code></li>
  <li>Post-Implementation style encouraged</li>
  <li>Coverage and integration tests coming soon...</li>
</ul>

<h2>🧼 Linting and Code Standards</h2>
<ul>
  <li>Follows Clean Architecture principles</li>
  <li>SOLID principles applied</li>
  <li>Centralized logging via Serilog</li>
</ul>

<h2>🙋‍♂️ Author</h2>
<p><strong>Ali</strong> – .NET Core Developer in Iran 🇮🇷<br>
Contact via GitHub or LinkedIn.<br>
Currently working on advanced .NET projects with a focus on clean, scalable, and production-ready systems.</p>
