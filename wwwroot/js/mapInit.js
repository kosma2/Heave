// Initialize the map on the "map" div with a given center and zoom level
var mymap = L.map('map').setView([51.505, -0.09], 13);

// Add a tile layer to add to our map
L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: 'Map data &copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
    maxZoom: 18,
}).addTo(mymap);

// Function to add a marker to the map
function addMarker(lat, lng, description) {
    var marker = L.marker([lat, lng]).addTo(mymap);
    marker.bindPopup(description);
}

// Fetch markers from the backend
fetch('/MapData/GetMarkers')
    .then(response => response.json())
    .then(data => {
        data.forEach(markerData => {
            addMarker(markerData.Lat, markerData.Lng, markerData.Description);
        });
    })
    .catch(error => console.error('Error fetching markers:', error));

    function addGeoJsonPoints(geoJsonArray) {
        geoJsonArray.forEach(jsonStr => {
            var geoJsonObject = JSON.parse(jsonStr);
            L.geoJSON(geoJsonObject).addTo(mymap);
        });
    }
    
    // Call this function with the geoJsonData passed from the view
    addGeoJsonPoints(geoJsonData);

   

