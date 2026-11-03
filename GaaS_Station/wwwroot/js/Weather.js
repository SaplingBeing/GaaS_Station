const apiKey = "ef9b791c5a29aeab564a63a0bd9323ef";
const latitude = "-36.8485";
const longitude = "174.7635"; 


document.addEventListener("DOMContentLoaded", async function() {
    try {
        const weatherData = await getWeatherData();
        displayWeather(weatherData);
    }
    catch(error) {
        console.error(error);
    }
})

async function getWeatherData() {
    const apiUrl = `https://api.openweathermap.org/data/2.5/weather?lat=${latitude}&lon=${longitude}&appid=${apiKey}&units=metric`;
    const response = await fetch(apiUrl);
    console.log(response);
    if (!response.ok) {
        throw new Error("Fetch failed");
    }
    const data = await response.json();
    return data;
}

function displayWeather(data) {
    const weatherContainer = document.querySelector("#weatherContainer");
    weatherContainer.innerHTML = `
        <h2 class="fw-bold">${data.name}</h2>
        <p class="m-0"><span class="fw-bold">Temperature:</span> ${data.main.temp}°C</p>
        <p class="m-0"><span class="fw-bold">Weather:</span> ${data.weather[0].description}</p>
        <p class="m-0"><span class="fw-bold">Humidity:</span> ${data.main.humidity}%</p>
    `;
}