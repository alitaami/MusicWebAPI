<h1>🎵 Music Web API</h1> <p> A powerful, containerized .NET Web API project inspired by <strong>Spotify</strong>, featuring built-in support for <strong>Hangfire scheduling</strong>, <strong>JWT-based authentication</strong>, <strong>Swagger for API documentation</strong>, <strong>SignalR real-time chat</strong>, and <strong>Kibana log monitoring</strong>. </p>

<h2>🚀 Project Features</h2>
<ul>
  <li>✅ JWT Authentication with role-based access</li>
  <li>✅ Hangfire Dashboard for background job monitoring</li>
  <li>✅ Swagger UI for easy API testing</li>
  <li>✅ Real-time chat with SignalR</li>
  <li>✅ ElasticSearch + Kibana integration for log monitoring</li>
  <li>✅ Clean, maintainable architecture (Clean Architecture)</li>
  <li>✅ Containerized with Docker and Docker Compose</li>
</ul>

<h2>🛠️ Technologies Used</h2>
<table>
  <tr><th>Stack</th><th>Details</th></tr>
  <tr><td><strong>Backend</strong></td><td>ASP.NET Core Web API (.NET 8)</td></tr>
  <tr><td><strong>Real-Time</strong></td><td>SignalR</td></tr>
  <tr><td><strong>Background Jobs</strong></td><td>Hangfire</td></tr>
  <tr><td><strong>Logging</strong></td><td>Serilog + Elasticsearch + Kibana</td></tr>
  <tr><td><strong>Auth</strong></td><td>JWT Bearer Token</td></tr>
  <tr><td><strong>Docs</strong></td><td>Swagger / OpenAPI</td></tr>
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

<h2>📄 Environment(.env) & AppSettings(appsettings.json) files </h2>
<p>
  The<code>.env & appsettings.json</code> files are not included in the repository. Developers who want to test the app can download it from the following Google Drive link:
</p>
<p>
  <a href="https://drive.google.com/file/d/1PISHbiUgCH54-B5mt7jEKzO2ubvsgrvK/view?usp=drive_link" target="_blank">
    👉 Access .env file
  </a>
  </p>
  <p>
    <a href="https://drive.google.com/file/d/1iysuL5EzC4rUrbHnlBEka4qJZUSlayZE/view?usp=drive_link" target="_blank">
    👉 Access appsettings.json file
  </a>
</p>
<p>
  After downloading, place the<code>.env & appsettings.json</code> files in the root of the <code>MusicWebApi.API</code> layer.
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
  </code> 
<code>/src
  ├── MusicWebAPI.API                  → Web entry point
  ├── MusicWebAPI.Application          → Business logic and use cases
  ├── MusicWebAPI.Domain               → Core domain models/entities
  ├── MusicWebAPI.Infrastructure       → EF Core, external services, logging
  └── MusicWebAPI.Core                 → Shared constants, utilities, resources
</code>
</pre>

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

<h2>📘 Swagger UI</h2>
<p><strong>URL:</strong> <a href="http://localhost:8080/swagger/index.html">http://localhost:8080/swagger/index.html</a></p>

<h2>💬 Real-Time Chat</h2>
<p><strong>URL:</strong> <a href="http://localhost:8080/chat.html">http://localhost:8080/chat.html</a></p>

<h2>🕒 Hangfire Dashboard</h2>
<p><strong>URL:</strong> <a href="http://localhost:8080/hangfire/">http://localhost:8080/hangfire/</a></p>
<p><strong>Note:</strong> Access is secured via JWT. Make sure your token includes the required roles (e.g., <code>SuperUser</code>).</p>

<h2>📊 Kibana Log Monitoring</h2>
<ol>
  <li>Visit Kibana dashboard (<code>http://localhost:5601</code> by default)</li>
  <li>Go to <strong>Management → Stack Management → Index Patterns</strong></li>
  <li>Create a new index pattern:
    <ul>
      <li><strong>Name:</strong> <code>musicwebapi-logs-*</code></li>
      <li><strong>Timestamp field:</strong> <code>@timestamp</code></li>
    </ul>
  </li>
  <li>Go to <strong>Discover</strong> to view logs</li>
</ol>

<h2>🧪 Testing</h2>
<ul>
  <li>Unit Tests: <code>NUnit</code></li>
  <li>TDD-style encouraged</li>
  <li>Coverage and integration tests coming soon...</li>
</ul>

<h2>🧼 Linting and Code Standards</h2>
<ul>
  <li>Follows Clean Architecture principles</li>
  <li>SOLID principles applied</li>
  <li>Centralized logging via Serilog</li>
</ul>

<h2>🛢️ Database Access</h2>
<p>If you want to connect to the database locally using <a href="https://www.beekeeperstudio.io/">Beekeeper Studio</a> or any PostgreSQL client, use the following credentials:</p>
<ul>
  <li><strong>Connection Type:</strong> PostgreSQL</li>
  <li><strong>Host:</strong> localhost</li>
  <li><strong>Port:</strong> 5432</li>
  <li><strong>User:</strong> sa</li>
  <li><strong>Password:</strong> sa123</li>
  <li><strong>Default Database:</strong> MusicDb</li>
</ul>

<h2>📜 License</h2>
<p>This project is licensed under the <strong>MIT License</strong>.</p>

<h2>🙋‍♂️ Author</h2>
<p><strong>Ali</strong> – .NET Core Developer in Iran 🇮🇷<br>
Contact via GitHub or LinkedIn.<br>
Currently working on advanced .NET projects with a focus on clean, scalable, and production-ready systems.</p>
