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


    
    function addWktPoint(wktString) {
        var coords = wktString.split('(')[1].split(')')[0].split(' ');
        var lng = parseFloat(coords[0]);
        var lat = parseFloat(coords[1]);
    
        var marker = L.marker([lat, lng]).addTo(map);
        map.setView([lat, lng], 13);
    }

    addWktPoint("POINT (-117.35232639217136 49.496907430701029)");

