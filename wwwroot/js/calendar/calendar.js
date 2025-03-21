// Initialize to today's date
let currentDate = new Date();
// Reset time to midnight to avoid timezone issues
currentDate.setHours(0, 0, 0, 0);
let calendarData = null;

// Fungsi untuk menambahkan hari ke date
function addDays(date, days) {
  const result = new Date(date);
  result.setDate(result.getDate() + days);
  return result;
}

// Fungsi untuk memformat tanggal untuk API
function formatDateForApi(date) {
  // Format tanggal untuk API, tetap menggunakan zona waktu lokal (WITA)
  // karena server akan mengkonversinya ke UTC
  return date.toISOString().split('T')[0];
}

// Fungsi untuk menginisialisasi kalender
function initializeCalendar() {
  updateDateHeaders();
  fetchCalendarData();

  // Disable prev button if we're at the current week
  const today = new Date();
  today.setHours(0, 0, 0, 0); // Reset time for accurate comparison
  document.getElementById('prevWeek').disabled = currentDate <= today;
}

// Fungsi untuk memperbarui header tanggal
function updateDateHeaders() {
  const dateHeaders = document.querySelectorAll('.date-header');
  for (let i = 0; i < 7; i++) {
    const date = addDays(currentDate, i);
    const dayName = date.toLocaleDateString('en-US', { weekday: 'short' });
    const dayNumber = date.getDate();
    dateHeaders[i].innerHTML = `
      <div class="day-name">${dayName}</div>
      <div class="day-number">${dayNumber}</div>
    `;
  }

  // Update date range text
  const startDate = currentDate.toLocaleDateString('en-US', { month: 'long', day: 'numeric' });
  const endDate = addDays(currentDate, 6).toLocaleDateString('en-US', {
    month: 'long',
    day: 'numeric',
    year: 'numeric'
  });
  document.getElementById('dateRangeText').textContent = `${startDate} - ${endDate}`;
}

// Fungsi untuk mengambil data kalender dari API
function fetchCalendarData() {
  // Show loading indicator
  document.getElementById('calendarLoading').style.display = 'block';
  document.getElementById('calendarContent').style.display = 'none';
  document.getElementById('calendarError').style.display = 'none';

  const startDateStr = formatDateForApi(currentDate);
  const endDateStr = formatDateForApi(addDays(currentDate, 6));

  fetch(`/api/Bookings/CalendarView?startDate=${startDateStr}&endDate=${endDateStr}`)
    .then(response => {
      if (!response.ok) {
        throw new Error('Network response was not ok');
      }
      return response.json();
    })
    .then(data => {
      console.log('Calendar API response:', data);

      // Proses data: tanggal dari server sudah dalam WITA
      calendarData = data;
      renderCalendar();

      // Hide loading, show content
      document.getElementById('calendarLoading').style.display = 'none';
      document.getElementById('calendarContent').style.display = 'flex';
    })
    .catch(error => {
      console.error('Error fetching calendar data:', error);
      // Show error message
      document.getElementById('calendarLoading').style.display = 'none';
      document.getElementById('calendarError').style.display = 'block';
    });
}

// Fungsi untuk merender kalender
function renderCalendar() {
  if (!calendarData || !calendarData.cranes) {
    console.error('No calendar data available');
    return;
  }

  // Render crane list
  const craneListEl = document.getElementById('craneList');
  craneListEl.innerHTML = '';

  // Render schedule rows
  const scheduleRowsEl = document.getElementById('scheduleRows');
  scheduleRowsEl.innerHTML = '';

  calendarData.cranes.forEach(crane => {
    // Add crane to the crane list
    const craneItem = document.createElement('div');
    craneItem.className = 'crane-item';
    craneItem.dataset.craneId = crane.craneId;
    craneItem.innerHTML = `
      <div class="p-2">
        <div class="fw-bold">${crane.craneId}</div>
        <div class="text-muted">${crane.capacity}</div>
      </div>
    `;
    craneListEl.appendChild(craneItem);

    // Create schedule row for this crane
    const scheduleRow = document.createElement('div');
    scheduleRow.className = 'schedule-row';
    scheduleRow.dataset.craneId = crane.craneId;

    // Create cells for each day of the week
    for (let i = 0; i < 7; i++) {
      const cell = document.createElement('div');
      cell.className = 'schedule-cell';
      cell.dataset.dateOffset = i;
      cell.innerHTML = `
        <div class="shift-container">
          <div class="day-shift-container"></div>
          <div class="night-shift-container"></div>
        </div>
      `;
      scheduleRow.appendChild(cell);
    }

    scheduleRowsEl.appendChild(scheduleRow);
  });

  // Populate bookings into cells
  renderBookings();
}

// Fungsi untuk merender booking ke dalam sel-sel kalender
function renderBookings() {
  if (!calendarData || !calendarData.cranes) return;

  calendarData.cranes.forEach(crane => {
    const bookings = crane.bookings || [];
    const row = document.querySelector(`.schedule-row[data-crane-id="${crane.craneId}"]`);

    if (!row) {
      console.warn(`Schedule row not found for crane ${crane.craneId}`);
      return;
    }

    // Loop through all bookings for this crane
    bookings.forEach(booking => {
      // Normalize the booking date - tanggal dari server sudah dalam WITA
      const bookingDate = new Date(booking.date);
      bookingDate.setHours(0, 0, 0, 0); // Reset time for accurate comparison

      // Calculate day offset from the start of the week (currentDate)
      const daysDiff = Math.floor((bookingDate - currentDate) / (24 * 60 * 60 * 1000));

      // Skip bookings outside the current week view
      if (daysDiff < 0 || daysDiff > 6) return;

      // Find the cell for this date
      const cell = row.querySelector(`.schedule-cell[data-date-offset="${daysDiff}"]`);
      if (!cell) return;

      // Add booking to day shift if applicable
      if (booking.isDayShift) {
        addBookingCard(cell, '.day-shift-container', booking, 'day-shift');
      }

      // Add booking to night shift if applicable
      if (booking.isNightShift) {
        addBookingCard(cell, '.night-shift-container', booking, 'night-shift');
      }
    });
  });
}

// Helper function untuk menambahkan card booking
function addBookingCard(cell, containerSelector, booking, shiftClass) {
  const container = cell.querySelector(containerSelector);
  const card = document.createElement('div');
  card.className = `booking-card ${shiftClass}`;
  card.title = `${booking.bookingNumber} - ${booking.department}`;

  const content = document.createElement('div');
  content.className = 'booking-card-content';
  content.textContent = booking.department;

  card.appendChild(content);
  container.appendChild(card);
}

// Fungsi untuk navigasi antar minggu
function navigateWeek(weeks) {
  const today = new Date();
  today.setHours(0, 0, 0, 0); // Reset time for accurate comparison
  const newDate = addDays(currentDate, weeks * 7);

  if (weeks < 0 && newDate < today) {
    return; // Prevent navigating to past weeks
  }

  currentDate = newDate;
  updateDateHeaders();
  fetchCalendarData();

  // Update prev button state
  const prevButton = document.getElementById('prevWeek');
  prevButton.disabled = currentDate <= today;

  // Reset scroll position ke awal (paling kiri)
  const scrollContainer = document.querySelector('.calendar-scroll-container');
  scrollContainer.scrollLeft = 0;
}
