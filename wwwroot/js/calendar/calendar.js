// Initialize to today's date
let currentDate = new Date();
// Reset time to midnight to avoid timezone issues
currentDate.setHours(0, 0, 0, 0);
let calendarData = null;
let shiftDefinitions = [];

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

  // Fetch shift definitions first, then calendar data
  fetchShiftDefinitions()
    .then(() => {
      fetchCalendarData();
    });

  // Disable prev button if we're at the current week
  const today = new Date();
  today.setHours(0, 0, 0, 0); // Reset time for accurate comparison
  document.getElementById('prevWeek').disabled = currentDate <= today;

  // Initialize tooltip container
  if (!document.getElementById('calendar-tooltip')) {
    const tooltip = document.createElement('div');
    tooltip.id = 'calendar-tooltip';
    tooltip.className = 'calendar-tooltip';
    document.body.appendChild(tooltip);
  }
}

// Fungsi untuk mengambil definisi shift dari API
async function fetchShiftDefinitions() {
  try {
    const response = await fetch('/api/ShiftDefinitions');
    if (!response.ok) {
      throw new Error('Failed to fetch shift definitions');
    }

    shiftDefinitions = await response.json();

    // Sort shift definitions by start time
    shiftDefinitions.sort((a, b) => {
      const timeA = a.startTime.split(':').map(Number);
      const timeB = b.startTime.split(':').map(Number);
      return (timeA[0] * 60 + timeA[1]) - (timeB[0] * 60 + timeB[1]);
    });

    return shiftDefinitions;
  } catch (error) {
    console.error('Error fetching shift definitions:', error);
    document.getElementById('calendarError').style.display = 'block';
    throw error;
  }
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
        <div class="text-muted">${crane.capacity} TON</div>
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

      // Create shift slots container
      const shiftsContainer = document.createElement('div');
      shiftsContainer.className = 'shift-container';

      // Create shift slots dynamically based on shift definitions
      if (shiftDefinitions.length > 0) {
        // Calculate the height percentage for each shift slot
        const slotHeightPercent = 100 / shiftDefinitions.length;

        shiftDefinitions.forEach(shift => {
          const shiftSlot = document.createElement('div');
          shiftSlot.className = 'shift-slot';
          shiftSlot.dataset.shiftId = shift.id;
          shiftSlot.style.height = `${slotHeightPercent}%`;
          shiftsContainer.appendChild(shiftSlot);
        });
      } else {
        // Default fallback if no shift definitions loaded
        const defaultSlot = document.createElement('div');
        defaultSlot.className = 'shift-slot';
        defaultSlot.style.height = '100%';
        shiftsContainer.appendChild(defaultSlot);
      }

      cell.appendChild(shiftsContainer);
      scheduleRow.appendChild(cell);
    }

    scheduleRowsEl.appendChild(scheduleRow);
  });

  // Populate bookings and maintenance schedules into cells
  renderBookings();
  renderMaintenanceSchedules();

  // Add event listeners for tooltips
  setupTooltips();
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

      // Process each shift in the booking
      booking.shifts.forEach(shift => {
        // Find the shift slot based on the shift definition ID
        const shiftSlot = cell.querySelector(`.shift-slot[data-shift-id="${shift.shiftDefinitionId}"]`);
        if (!shiftSlot) return;

        // Determine department class for styling
        let deptClass = 'dept-default';
        const department = booking.department.toLowerCase();

        if (department.includes('stores') || department.includes('inventory')) {
          deptClass = 'dept-stores';
        }

        // Create booking card
        const card = document.createElement('div');
        card.className = `booking-card ${deptClass}`;
        card.dataset.bookingId = booking.id;
        card.dataset.bookingNumber = booking.bookingNumber;
        card.dataset.department = booking.department;
        card.dataset.shiftName = shift.shiftName;
        card.dataset.startTime = shift.startTime;
        card.dataset.endTime = shift.endTime;

        const content = document.createElement('div');
        content.className = 'booking-card-content';
        content.textContent = booking.department;

        card.appendChild(content);
        shiftSlot.appendChild(card);
      });
    });
  });
}

// Fungsi untuk merender jadwal maintenance ke dalam sel-sel kalender
function renderMaintenanceSchedules() {
  if (!calendarData || !calendarData.cranes) return;

  calendarData.cranes.forEach(crane => {
    const maintenanceSchedules = crane.maintenanceSchedules || [];
    const row = document.querySelector(`.schedule-row[data-crane-id="${crane.craneId}"]`);

    if (!row) {
      console.warn(`Schedule row not found for crane ${crane.craneId}`);
      return;
    }

    // Loop through all maintenance schedules for this crane
    maintenanceSchedules.forEach(maintenance => {
      // Normalize the maintenance date
      const maintenanceDate = new Date(maintenance.date);
      maintenanceDate.setHours(0, 0, 0, 0); // Reset time for accurate comparison

      // Calculate day offset from the start of the week (currentDate)
      const daysDiff = Math.floor((maintenanceDate - currentDate) / (24 * 60 * 60 * 1000));

      // Skip maintenance outside the current week view
      if (daysDiff < 0 || daysDiff > 6) return;

      // Find the cell for this date
      const cell = row.querySelector(`.schedule-cell[data-date-offset="${daysDiff}"]`);
      if (!cell) return;

      // Process each shift in the maintenance schedule
      maintenance.shifts.forEach(shift => {
        // Find the shift slot based on the shift definition ID
        const shiftSlot = cell.querySelector(`.shift-slot[data-shift-id="${shift.shiftDefinitionId}"]`);
        if (!shiftSlot) {
          console.warn(`Shift slot not found for shift definition ID ${shift.shiftDefinitionId}`);
          return;
        }

        // Create maintenance card
        const card = document.createElement('div');
        card.className = 'maintenance-card';
        card.dataset.maintenanceId = maintenance.id;
        card.dataset.maintenanceTitle = maintenance.title;
        card.dataset.shiftName = shift.shiftName;
        card.dataset.startTime = shift.startTime;
        card.dataset.endTime = shift.endTime;

        const title = document.createElement('div');
        title.className = 'maintenance-title';
        title.textContent = maintenance.title;

        card.appendChild(title);
        shiftSlot.appendChild(card);
      });
    });
  });
}

// Fungsi untuk mengatur tooltips
function setupTooltips() {
  const tooltip = document.getElementById('calendar-tooltip');

  // Add event listeners to booking cards
  document.querySelectorAll('.booking-card').forEach(card => {
    card.addEventListener('mouseenter', function(e) {
      const bookingNumber = this.dataset.bookingNumber;
      const department = this.dataset.department;
      const shiftName = this.dataset.shiftName;
      const startTime = this.dataset.startTime ? this.dataset.startTime.substring(0, 5) : ''; // Format HH:MM
      const endTime = this.dataset.endTime ? this.dataset.endTime.substring(0, 5) : ''; // Format HH:MM

      tooltip.innerHTML = `
        <div class="calendar-tooltip-title">${bookingNumber}</div>
        <div class="calendar-tooltip-content">
          <div>Department: ${department}</div>
          <div>Shift: ${shiftName}</div>
          <div>Time: ${startTime} - ${endTime}</div>
        </div>
      `;

      // Position tooltip near the cursor
      tooltip.style.left = (e.pageX + 10) + 'px';
      tooltip.style.top = (e.pageY + 10) + 'px';
      tooltip.style.display = 'block';
    });

    card.addEventListener('mouseleave', function() {
      tooltip.style.display = 'none';
    });
  });

  // Add event listeners to maintenance cards
  document.querySelectorAll('.maintenance-card').forEach(card => {
    card.addEventListener('mouseenter', function(e) {
      const title = this.dataset.maintenanceTitle;
      const shiftName = this.dataset.shiftName;
      const startTime = this.dataset.startTime ? this.dataset.startTime.substring(0, 5) : ''; // Format HH:MM
      const endTime = this.dataset.endTime ? this.dataset.endTime.substring(0, 5) : ''; // Format HH:MM

      tooltip.innerHTML = `
        <div class="calendar-tooltip-title">Maintenance</div>
        <div class="calendar-tooltip-content">
          <div>Title: ${title}</div>
          <div>Shift: ${shiftName}</div>
          <div>Time: ${startTime} - ${endTime}</div>
        </div>
      `;

      // Position tooltip near the cursor
      tooltip.style.left = (e.pageX + 10) + 'px';
      tooltip.style.top = (e.pageY + 10) + 'px';
      tooltip.style.display = 'block';
    });

    card.addEventListener('mouseleave', function() {
      tooltip.style.display = 'none';
    });
  });
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

  // No need to fetch shift definitions again if we already have them
  if (shiftDefinitions.length > 0) {
    fetchCalendarData();
  } else {
    fetchShiftDefinitions()
      .then(() => {
        fetchCalendarData();
      });
  }

  // Update prev button state
  const prevButton = document.getElementById('prevWeek');
  prevButton.disabled = currentDate <= today;

  // Reset scroll position ke awal (paling kiri)
  const scrollContainer = document.querySelector('.calendar-scroll-container');
  scrollContainer.scrollLeft = 0;
}
