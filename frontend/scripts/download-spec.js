const fs = require('fs');
const http = require('http');

const url = 'http://localhost:5168/openapi/v1.json';
const file = 'openapi.json';

http.get(url, (res) => {
    let data = '';
    res.on('data', (chunk) => {
        data += chunk;
    });
    res.on('end', () => {
        // Fix known .NET 9 OpenAPI bug with duplicate array schemas producing invalid $refs
        data = data.replace(/"#\/components\/schemas\/#\/properties\/newDirectors\/items"/g, '"#/components/schemas/RookieDto"');
        data = data.replace(/"#\/components\/schemas\/#\/properties\/topRated\/items"/g, '"#/components/schemas/WrappedMovieDto2"');
        
        fs.writeFileSync(file, data, 'utf8');
        console.log('Downloaded and sanitized openapi.json');
    });
}).on('error', (err) => {
    console.error('Error downloading spec: ' + err.message);
});
