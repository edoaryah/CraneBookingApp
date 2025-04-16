// DOM Elements
const loadingIndicator = document.getElementById('loadingIndicator');
const bookingTableContainer = document.getElementById('bookingTableContainer');
const bookingTableBody = document.getElementById('bookingTableBody');
const noDataMessage = document.getElementById('noDataMessage');
const errorMessage = document.getElementById('errorMessage');

// Global variables
let bookings = [];
let cranes = [];

// Initialize when the DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
  loadBookingHistory();
});

// Function to load booking history
async function loadBookingHistory() {
  showLoading();

  try {
    // Fetch both bookings and cranes in parallel
    const [bookingsResponse, cranesResponse] = await Promise.all([fetch('/api/Bookings'), fetch('/api/Cranes')]);

    if (!bookingsResponse.ok || !cranesResponse.ok) {
      throw new Error('Failed to fetch data');
    }

    bookings = await bookingsResponse.json();
    cranes = await cranesResponse.json();

    // Map cranes by ID for easier lookup
    const craneMap = {};
    cranes.forEach(crane => {
      craneMap[crane.id] = crane;
    });

    // Sort bookings by start date (newest first)
    bookings.sort((a, b) => new Date(b.startDate) - new Date(a.startDate));

    if (bookings.length === 0) {
      showNoData();
      return;
    }

    // Render the table
    bookingTableBody.innerHTML = '';

    bookings.forEach(booking => {
      const row = document.createElement('tr');

      // Format dates
      const startDate = new Date(booking.startDate);
      const endDate = new Date(booking.endDate);
      const formattedStartDate = formatDate(startDate);
      const formattedEndDate = formatDate(endDate);
      const dateRange = `${formattedStartDate} - ${formattedEndDate}`;

      // Get crane code
      const crane = craneMap[booking.craneId];
      const craneCode = crane ? crane.code : 'Unknown';

      // Populate row
      row.innerHTML = `
        <td>${booking.bookingNumber}</td>
        <td>${booking.name}</td>
        <td>${booking.department}</td>
        <td>${dateRange}</td>
        <td>${craneCode}</td>
        <td>
          <div class="dropdown">
            <button type="button" class="btn p-0 dropdown-toggle hide-arrow" data-bs-toggle="dropdown">
              <i class="bx bx-dots-vertical-rounded"></i>
            </button>
            <div class="dropdown-menu">
              <a class="dropdown-item" href="/BookingHistory/Details/${booking.id}">
                <i class="bx bx-show-alt me-1"></i> View Details
              </a>
              <a class="dropdown-item" href="/CraneUsage/Index/${booking.id}">
                <i class="bx bx-time me-1"></i> Crane Usage
              </a>
              <a class="dropdown-item" href="javascript:void(0);" onclick="event.stopPropagation();">
                <i class="bx bx-edit-alt me-1"></i> Edit
              </a>
              <a class="dropdown-item" href="javascript:void(0);" onclick="event.stopPropagation();">
                <i class="bx bx-trash me-1"></i> Delete
              </a>
            </div>
          </div>
        </td>
      `;

      // Add click event to the entire row
      row.style.cursor = 'pointer';
      row.addEventListener('click', function (e) {
        // Only navigate if we didn't click on a dropdown item
        if (!e.target.closest('.dropdown-item')) {
          window.location.href = `/BookingHistory/Details/${booking.id}`;
        }
      });

      bookingTableBody.appendChild(row);
    });

    showTable();
  } catch (error) {
    console.error('Error loading booking history:', error);
    showError();
  }
}

// Format date to DD-MM-YYYY
function formatDate(date) {
  const day = date.getDate().toString().padStart(2, '0');
  const month = (date.getMonth() + 1).toString().padStart(2, '0');
  const year = date.getFullYear();
  return `${day}-${month}-${year}`;
}

// UI helper functions
function showLoading() {
  loadingIndicator.style.display = 'block';
  bookingTableContainer.style.display = 'none';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'none';
}

function showTable() {
  loadingIndicator.style.display = 'none';
  bookingTableContainer.style.display = 'block';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'none';
}

function showNoData() {
  loadingIndicator.style.display = 'none';
  bookingTableContainer.style.display = 'none';
  noDataMessage.style.display = 'block';
  errorMessage.style.display = 'none';
}

function showError() {
  loadingIndicator.style.display = 'none';
  bookingTableContainer.style.display = 'none';
  noDataMessage.style.display = 'none';
  errorMessage.style.display = 'block';
}
