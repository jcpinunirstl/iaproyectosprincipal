import EventoAPI from './api.js';

class App {
    constructor() {
        this.currentUser = null;
        this.eventos = [];
        this.tipoEventos = [];
        this.personas = [];
        this.asistencias = [];
        this.init();
    }

    async init() {
        this.checkAuth();
        this.setupEventListeners();
        await this.loadPublicEvents();
    }

    checkAuth() {
        const token = localStorage.getItem('jwtToken');
        if (token) {
            this.currentUser = JSON.parse(localStorage.getItem('userData') || '{}');
            this.showAdminView();
        } else {
            this.showPublicView();
        }
    }

    setupEventListeners() {
        // Auth buttons
        document.getElementById('loginBtn').addEventListener('click', () => this.showLoginModal());
        document.getElementById('registerBtn').addEventListener('click', () => this.showRegisterModal());
        document.getElementById('logoutBtn').addEventListener('click', () => this.logout());

        // Forms
        document.getElementById('loginForm').addEventListener('submit', (e) => this.handleLogin(e));
        document.getElementById('registerForm').addEventListener('submit', (e) => this.handleRegister(e));

        // Tabs
        document.querySelectorAll('.tab-btn').forEach(btn => {
            btn.addEventListener('click', (e) => this.switchTab(e.target.dataset.tab));
        });

        // CRUD events
        document.getElementById('createBtn').addEventListener('click', () => this.showCreateForm());
        document.getElementById('eventForm').addEventListener('submit', (e) => this.handleSubmit(e));

        document.getElementById('createTipoEventoBtn').addEventListener('click', () => this.showCreateTipoEventoForm());
        document.getElementById('tipoEventoForm').addEventListener('submit', (e) => this.handleTipoEventoSubmit(e));

        document.getElementById('createAsistenciaBtn').addEventListener('click', () => this.showCreateAsistenciaForm());
        document.getElementById('asistenciaForm').addEventListener('submit', (e) => this.handleAsistenciaSubmit(e));

        document.getElementById('createPersonaBtn').addEventListener('click', () => this.showCreatePersonaForm());
        document.getElementById('personaForm').addEventListener('submit', (e) => this.handlePersonaSubmit(e));
    }

    showPublicView() {
        document.getElementById('publicView').style.display = 'block';
        document.getElementById('adminView').style.display = 'none';
    }

    showAdminView() {
        document.getElementById('publicView').style.display = 'none';
        document.getElementById('adminView').style.display = 'block';
        this.loadAdminData();
    }

    async loadPublicEvents() {
        try {
            console.log('Iniciando carga de eventos públicos...');
            this.eventos = await EventoAPI.getEventos();
            console.log('Eventos cargados exitosamente:', this.eventos);
            this.renderPublicEvents();
        } catch (error) {
            console.error('Error cargando eventos públicos:', error);
            const list = document.getElementById('publicEventsList');
            if (list) {
                list.innerHTML = `
                    <div class="alert alert-danger" role="alert">
                        <h4 class="alert-heading">Error al cargar eventos</h4>
                        <p>${error.message}</p>
                        <hr>
                        <p class="mb-0">
                            <strong>Posibles soluciones:</strong><br>
                            1. Verifica que el servidor API esté corriendo en http://localhost:5142<br>
                            2. Abre la consola del navegador (F12) para más detalles<br>
                            3. Revisa la configuración de CORS en el servidor
                        </p>
                    </div>
                `;
            }
        }
    }

    renderPublicEvents() {
        const list = document.getElementById('publicEventsList');
        list.innerHTML = '';
        this.eventos.forEach(evento => {
            const div = document.createElement('div');
            div.className = 'col-md-6 col-lg-4 mb-4';
            div.innerHTML = `
                <div class="card h-100 shadow-sm">
                    <div class="card-body">
                        <h5 class="card-title">${evento.nombre}</h5>
                        <p class="card-text">${evento.descripcion}</p>
                        <ul class="list-unstyled">
                            <li><i class="bi bi-geo-alt"></i> <strong>Dirección:</strong> ${evento.direccion}</li>
                            <li><i class="bi bi-cash"></i> <strong>Costo:</strong> $${evento.costo}</li>
                            <li><i class="bi bi-calendar"></i> <strong>Fecha:</strong> ${evento.fechaInicio} - ${evento.fechaFin}</li>
                            <li><i class="bi bi-clock"></i> <strong>Hora:</strong> ${evento.horaInicio} - ${evento.horaFin}</li>
                            <li><i class="bi bi-tag"></i> <strong>Tipo:</strong> ${evento.tipoEvento ? evento.tipoEvento.nombre : 'N/A'}</li>
                        </ul>
                    </div>
                </div>
            `;
            list.appendChild(div);
        });
    }

    showLoginModal() {
        const modalEl = document.getElementById('loginModal');
        const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
        modal.show();
    }

    hideLoginModal() {
        const modalEl = document.getElementById('loginModal');
        const modal = bootstrap.Modal.getInstance(modalEl);
        if (modal) modal.hide();
    }

    showRegisterModal() {
        const modalEl = document.getElementById('registerModal');
        const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
        modal.show();
    }

    hideRegisterModal() {
        const modalEl = document.getElementById('registerModal');
        const modal = bootstrap.Modal.getInstance(modalEl);
        if (modal) modal.hide();
    }

    async handleLogin(e) {
        e.preventDefault();
        const formData = new FormData(e.target);
        const credentials = {
            username: formData.get('username'),
            password: formData.get('password')
        };

        try {
            const response = await EventoAPI.login(credentials);
            localStorage.setItem('jwtToken', response.token);
            localStorage.setItem('userData', JSON.stringify(response));
            this.currentUser = response;
            this.hideLoginModal();
            this.showAdminView();
        } catch (error) {
            alert(error.message);
        }
    }

    async handleRegister(e) {
        e.preventDefault();
        const formData = new FormData(e.target);
        const userData = {
            username: formData.get('username'),
            password: formData.get('password'),
            nombre: formData.get('nombre'),
            telefono: formData.get('telefono'),
            fechaNacimiento: formData.get('fechaNacimiento'),
            genero: parseInt(formData.get('genero')) || 0,
            rol: 'usuario'
        };

        try {
            const response = await EventoAPI.register(userData);
            localStorage.setItem('jwtToken', response.token);
            localStorage.setItem('userData', JSON.stringify(response));
            this.currentUser = response;
            this.hideRegisterModal();
            this.showAdminView();
        } catch (error) {
            alert(error.message);
        }
    }

    logout() {
        localStorage.removeItem('jwtToken');
        localStorage.removeItem('userData');
        this.currentUser = null;
        this.showPublicView();
    }

    async loadAdminData() {
        await Promise.all([
            this.loadTipoEventos(),
            this.loadTipoEventosList(),
            this.loadEventos(),
            this.loadPersonas(),
            this.loadPersonasList(),
            this.loadAsistencias()
        ]);
    }

    switchTab(tab) {
        document.querySelectorAll('.tab-btn').forEach(btn => btn.classList.remove('active'));
        document.querySelectorAll('.tab-content').forEach(content => content.classList.remove('active'));

        document.querySelector(`[data-tab="${tab}"]`).classList.add('active');
        document.getElementById(`${tab}Tab`).classList.add('active');
    }

    // Eventos
    async loadTipoEventos() {
        try {
            this.tipoEventos = await EventoAPI.getTipoEventos();
            this.populateTipoEventoSelect();
        } catch (error) {
            console.error('Error cargando tipos de evento:', error);
        }
    }

    populateTipoEventoSelect() {
        const select = document.getElementById('tipoEventoId');
        select.innerHTML = '';
        this.tipoEventos.forEach(tipo => {
            const option = document.createElement('option');
            option.value = tipo.id;
            option.textContent = tipo.nombre;
            select.appendChild(option);
        });
    }

    populateEventoSelect() {
        const asistenciaSelect = document.getElementById('asistenciaEventoId');
        asistenciaSelect.innerHTML = '';
        this.eventos.forEach(evento => {
            const option = document.createElement('option');
            option.value = evento.id;
            option.textContent = evento.nombre;
            asistenciaSelect.appendChild(option);
        });
    }

    async loadEventos() {
        try {
            const token = localStorage.getItem('jwtToken');
            this.eventos = token ? await EventoAPI.getEventosUsuario() : await EventoAPI.getEventos();
            this.renderEventos();
            this.populateEventoSelect();
        } catch (error) {
            console.error('Error cargando eventos:', error);
        }
    }

    renderEventos() {
        const body = document.getElementById('eventsBody');
        body.innerHTML = '';
        this.eventos.forEach(evento => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${evento.id}</td>
                <td>${evento.nombre}</td>
                <td>${evento.descripcion}</td>
                <td>${evento.direccion}</td>
                <td>${evento.costo}</td>
                <td>${evento.fechaInicio}</td>
                <td>${evento.fechaFin}</td>
                <td>${evento.horaInicio}</td>
                <td>${evento.horaFin}</td>
                <td>${evento.tipoEvento ? evento.tipoEvento.nombre : ''}</td>
                <td>${evento.usuario ? evento.usuario.nombre : 'Sin asignar'}</td>
                <td><span class="badge ${evento.estado ? 'bg-success' : 'bg-secondary'}">${evento.estado ? 'Activo' : 'Inactivo'}</span></td>
                <td>
                    <button class="btn btn-sm btn-primary btn-edit"><i class="bi bi-pencil"></i> Editar</button>
                    <button class="btn btn-sm btn-danger btn-delete"><i class="bi bi-trash"></i> Eliminar</button>
                </td>
            `;
            row.querySelector('.btn-edit').addEventListener('click', () => this.editEvento(evento.id));
            row.querySelector('.btn-delete').addEventListener('click', () => this.deleteEvento(evento.id));
            body.appendChild(row);
        });
    }

    showCreateForm() {
        document.getElementById('formTitle').textContent = 'Crear Evento';
        document.getElementById('eventForm').reset();
        document.getElementById('eventId').value = '';
        const modalEl = document.getElementById('formModal');
        const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
        modal.show();
    }

    async editEvento(id) {
        try {
            const evento = await EventoAPI.getEvento(id);
            document.getElementById('formTitle').textContent = 'Editar Evento';
            document.getElementById('eventId').value = evento.id;
            document.getElementById('nombre').value = evento.nombre;
            document.getElementById('descripcion').value = evento.descripcion;
            document.getElementById('direccion').value = evento.direccion;
            document.getElementById('costo').value = evento.costo;
            document.getElementById('fechaInicio').value = evento.fechaInicio;
            document.getElementById('fechaFin').value = evento.fechaFin;
            document.getElementById('horaInicio').value = evento.horaInicio;
            document.getElementById('horaFin').value = evento.horaFin;
            document.getElementById('tipoEventoId').value = evento.tipoEventoId;
            document.getElementById('estado').checked = evento.estado;
            const modalEl = document.getElementById('formModal');
            const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
            modal.show();
        } catch (error) {
            console.error('Error cargando evento:', error);
        }
    }

    async deleteEvento(id) {
        if (confirm('¿Eliminar evento?')) {
            try {
                await EventoAPI.deleteEvento(id);
                await this.loadEventos();
            } catch (error) {
                alert('Error eliminando evento');
            }
        }
    }

    async handleSubmit(e) {
        e.preventDefault();
        const formData = new FormData(e.target);
        
        const usuarioId = this.currentUser ? (this.currentUser.usuarioId || this.currentUser.id) : null;
        
        if (!usuarioId) {
            alert('Error: Usuario no autenticado correctamente. Por favor, vuelve a iniciar sesión.');
            console.error('currentUser:', this.currentUser);
            return;
        }
        
        const evento = {
            id: parseInt(formData.get('id')) || 0,
            nombre: formData.get('nombre'),
            descripcion: formData.get('descripcion'),
            direccion: formData.get('direccion'),
            costo: parseFloat(formData.get('costo')),
            fechaInicio: formData.get('fechaInicio'),
            fechaFin: formData.get('fechaFin'),
            horaInicio: formData.get('horaInicio'),
            horaFin: formData.get('horaFin'),
            tipoEventoId: parseInt(formData.get('tipoEventoId')),
            usuarioId: usuarioId,
            estado: formData.has('estado'),
        };

        try {
            if (evento.id) {
                await EventoAPI.updateEvento(evento.id, evento);
            } else {
                await EventoAPI.createEvento(evento);
            }
            this.hideModal();
            await this.loadEventos();
        } catch (error) {
            alert(error.message);
        }
    }

    hideModal() {
        const modalEl = document.getElementById('formModal');
        const modal = bootstrap.Modal.getInstance(modalEl);
        if (modal) modal.hide();
    }

    // Tipo Eventos
    async loadTipoEventosList() {
        try {
            this.tipoEventos = await EventoAPI.getTipoEventos();
            this.renderTipoEventos();
        } catch (error) {
            console.error('Error cargando tipos de evento:', error);
        }
    }

    renderTipoEventos() {
        const body = document.getElementById('tipoEventosBody');
        body.innerHTML = '';
        this.tipoEventos.forEach(tipo => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${tipo.id}</td>
                <td>${tipo.nombre}</td>
                <td><span class="badge ${tipo.estado ? 'bg-success' : 'bg-secondary'}">${tipo.estado ? 'Activo' : 'Inactivo'}</span></td>
                <td>
                    <button class="btn btn-sm btn-primary btn-edit"><i class="bi bi-pencil"></i> Editar</button>
                    <button class="btn btn-sm btn-danger btn-delete"><i class="bi bi-trash"></i> Eliminar</button>
                </td>
            `;
            row.querySelector('.btn-edit').addEventListener('click', () => this.editTipoEvento(tipo.id));
            row.querySelector('.btn-delete').addEventListener('click', () => this.deleteTipoEvento(tipo.id));
            body.appendChild(row);
        });
    }

    showCreateTipoEventoForm() {
        document.getElementById('tipoEventoFormTitle').textContent = 'Crear Tipo de Evento';
        document.getElementById('tipoEventoForm').reset();
        document.getElementById('tipoEventoFormId').value = '';
        const modalEl = document.getElementById('tipoEventoModal');
        const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
        modal.show();
    }

    async editTipoEvento(id) {
        try {
            const tipo = await EventoAPI.getTipoEvento(id);
            document.getElementById('tipoEventoFormTitle').textContent = 'Editar Tipo de Evento';
            document.getElementById('tipoEventoFormId').value = tipo.id;
            document.getElementById('tipoEventoNombre').value = tipo.nombre;
            document.getElementById('tipoEventoEstado').checked = tipo.estado;
            const modalEl = document.getElementById('tipoEventoModal');
            const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
            modal.show();
        } catch (error) {
            console.error('Error cargando tipo de evento:', error);
        }
    }

    async deleteTipoEvento(id) {
        if (confirm('¿Eliminar tipo de evento?')) {
            try {
                await EventoAPI.deleteTipoEvento(id);
                await this.loadTipoEventosList();
            } catch (error) {
                alert('Error eliminando tipo de evento');
            }
        }
    }

    async handleTipoEventoSubmit(e) {
        e.preventDefault();
        const formData = new FormData(e.target);
        const tipoEvento = {
            id: parseInt(formData.get('id')) || 0,
            nombre: formData.get('nombre'),
            estado: formData.has('estado'),
        };

        try {
            if (tipoEvento.id) {
                await EventoAPI.updateTipoEvento(tipoEvento.id, tipoEvento);
            } else {
                await EventoAPI.createTipoEvento(tipoEvento);
            }
            this.hideTipoEventoModal();
            await this.loadTipoEventosList();
        } catch (error) {
            alert(error.message);
        }
    }

    hideTipoEventoModal() {
        const modalEl = document.getElementById('tipoEventoModal');
        const modal = bootstrap.Modal.getInstance(modalEl);
        if (modal) modal.hide();
    }

    // Personas
    async loadPersonasList() {
        try {
            this.personas = await EventoAPI.getPersonas();
            this.renderPersonas();
        } catch (error) {
            console.error('Error cargando personas:', error);
        }
    }

    renderPersonas() {
        const body = document.getElementById('personasBody');
        body.innerHTML = '';
        this.personas.forEach(persona => {
            const row = document.createElement('tr');
            const generoTexto = persona.genero === 0 ? 'Masculino' : persona.genero === 1 ? 'Femenino' : 'Otro';
            row.innerHTML = `
                <td>${persona.id}</td>
                <td>${persona.nombre}</td>
                <td>${persona.telefono || ''}</td>
                <td>${persona.fechaNacimiento ? new Date(persona.fechaNacimiento).toLocaleDateString() : ''}</td>
                <td>${generoTexto}</td>
                <td><span class="badge ${persona.estado ? 'bg-success' : 'bg-secondary'}">${persona.estado ? 'Activo' : 'Inactivo'}</span></td>
                <td>
                    <button class="btn btn-sm btn-primary btn-edit"><i class="bi bi-pencil"></i> Editar</button>
                    <button class="btn btn-sm btn-danger btn-delete"><i class="bi bi-trash"></i> Eliminar</button>
                </td>
            `;
            row.querySelector('.btn-edit').addEventListener('click', () => this.editPersona(persona.id));
            row.querySelector('.btn-delete').addEventListener('click', () => this.deletePersona(persona.id));
            body.appendChild(row);
        });
    }

    showCreatePersonaForm() {
        document.getElementById('personaFormTitle').textContent = 'Crear Persona';
        document.getElementById('personaForm').reset();
        document.getElementById('personaId').value = '';
        const modalEl = document.getElementById('personaModal');
        const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
        modal.show();
    }

    async editPersona(id) {
        try {
            const persona = await EventoAPI.getPersona(id);
            document.getElementById('personaFormTitle').textContent = 'Editar Persona';
            document.getElementById('personaId').value = persona.id;
            document.getElementById('personaNombre').value = persona.nombre;
            document.getElementById('personaTelefono').value = persona.telefono || '';
            document.getElementById('personaFechaNacimiento').value = persona.fechaNacimiento ? persona.fechaNacimiento.split('T')[0] : '';
            document.getElementById('personaGenero').value = persona.genero;
            document.getElementById('personaEstado').checked = persona.estado;
            const modalEl = document.getElementById('personaModal');
            const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
            modal.show();
        } catch (error) {
            console.error('Error cargando persona:', error);
        }
    }

    async deletePersona(id) {
        if (confirm('¿Eliminar persona?')) {
            try {
                await EventoAPI.deletePersona(id);
                await this.loadPersonasList();
            } catch (error) {
                alert('Error eliminando persona');
            }
        }
    }

    async handlePersonaSubmit(e) {
        e.preventDefault();
        const formData = new FormData(e.target);
        const persona = {
            id: parseInt(formData.get('id')) || 0,
            nombre: formData.get('nombre'),
            telefono: formData.get('telefono'),
            fechaNacimiento: formData.get('fechaNacimiento'),
            genero: parseInt(formData.get('genero')) || 0,
            estado: formData.has('estado'),
        };

        try {
            if (persona.id) {
                await EventoAPI.updatePersona(persona.id, persona);
            } else {
                await EventoAPI.createPersona(persona);
            }
            this.hidePersonaModal();
            await this.loadPersonasList();
        } catch (error) {
            alert(error.message);
        }
    }

    hidePersonaModal() {
        const modalEl = document.getElementById('personaModal');
        const modal = bootstrap.Modal.getInstance(modalEl);
        if (modal) modal.hide();
    }

    // Asistencias
    async loadPersonas() {
        try {
            this.personas = await EventoAPI.getPersonas();
            this.populatePersonaSelect();
        } catch (error) {
            console.error('Error cargando personas:', error);
        }
    }

    populatePersonaSelect() {
        const select = document.getElementById('asistenciaPersonaId');
        select.innerHTML = '';
        this.personas.forEach(persona => {
            const option = document.createElement('option');
            option.value = persona.id;
            option.textContent = persona.nombre;
            select.appendChild(option);
        });
    }

    async loadAsistencias() {
        try {
            this.asistencias = await EventoAPI.getRegistroAsistencias();
            this.renderAsistencias();
        } catch (error) {
            console.error('Error cargando asistencias:', error);
        }
    }

    renderAsistencias() {
        const body = document.getElementById('asistenciasBody');
        body.innerHTML = '';
        this.asistencias.forEach(asistencia => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${asistencia.id}</td>
                <td>${asistencia.fechaEntrada}</td>
                <td>${asistencia.observacion || ''}</td>
                <td>${asistencia.evento ? asistencia.evento.nombre : ''}</td>
                <td>${asistencia.persona ? asistencia.persona.nombre : ''}</td>
                <td>
                    <button class="btn btn-sm btn-primary btn-edit"><i class="bi bi-pencil"></i> Editar</button>
                    <button class="btn btn-sm btn-danger btn-delete"><i class="bi bi-trash"></i> Eliminar</button>
                </td>
            `;
            row.querySelector('.btn-edit').addEventListener('click', () => this.editAsistencia(asistencia.id));
            row.querySelector('.btn-delete').addEventListener('click', () => this.deleteAsistencia(asistencia.id));
            body.appendChild(row);
        });
    }

    showCreateAsistenciaForm() {
        document.getElementById('asistenciaFormTitle').textContent = 'Crear Asistencia';
        document.getElementById('asistenciaForm').reset();
        document.getElementById('asistenciaId').value = '';
        const modalEl = document.getElementById('asistenciaModal');
        const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
        modal.show();
    }

    async editAsistencia(id) {
        try {
            const asistencia = await EventoAPI.getRegistroAsistencia(id);
            document.getElementById('asistenciaFormTitle').textContent = 'Editar Asistencia';
            document.getElementById('asistenciaId').value = asistencia.id;
            document.getElementById('asistenciaFechaEntrada').value = asistencia.fechaEntrada;
            document.getElementById('asistenciaObservacion').value = asistencia.observacion || '';
            document.getElementById('asistenciaEventoId').value = asistencia.eventoId;
            document.getElementById('asistenciaPersonaId').value = asistencia.personaId;
            const modalEl = document.getElementById('asistenciaModal');
            const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
            modal.show();
        } catch (error) {
            console.error('Error cargando asistencia:', error);
        }
    }

    async deleteAsistencia(id) {
        if (confirm('¿Eliminar asistencia?')) {
            try {
                await EventoAPI.deleteRegistroAsistencia(id);
                await this.loadAsistencias();
            } catch (error) {
                alert('Error eliminando asistencia');
            }
        }
    }

    async handleAsistenciaSubmit(e) {
        e.preventDefault();
        const formData = new FormData(e.target);
        const asistencia = {
            id: parseInt(formData.get('id')) || 0,
            fechaEntrada: formData.get('fechaEntrada'),
            observacion: formData.get('observacion'),
            eventoId: parseInt(formData.get('eventoId')),
            personaId: parseInt(formData.get('personaId')),
        };

        try {
            if (asistencia.id) {
                await EventoAPI.updateRegistroAsistencia(asistencia.id, asistencia);
            } else {
                await EventoAPI.createRegistroAsistencia(asistencia);
            }
            this.hideAsistenciaModal();
            await this.loadAsistencias();
        } catch (error) {
            alert(error.message);
        }
    }

    hideAsistenciaModal() {
        const modalEl = document.getElementById('asistenciaModal');
        const modal = bootstrap.Modal.getInstance(modalEl);
        if (modal) modal.hide();
    }
}

new App();