// Detectar entorno y configurar URL base del API
const getApiBaseUrl = () => {
    // Si estamos en file:// (abriendo directamente el HTML), usar localhost:5142
    if (window.location.protocol === 'file:') {
        return 'http://localhost:5142/api';
    }
    
    // Si hay un servidor web sirviendo en cualquier puerto, usar URL absoluta
    // Solo usar ruta relativa si explícitamente estamos en producción con variable
    const isProduction = window.location.hostname !== 'localhost' && window.location.hostname !== '127.0.0.1';
    
    if (isProduction) {
        return '/api';  // En producción/Docker usar proxy
    }
    
    // En desarrollo local, siempre usar URL completa
    return 'http://localhost:5142/api';
};

const API_BASE_URL = getApiBaseUrl();
console.log('API Base URL configurada:', API_BASE_URL);
console.log('Hostname:', window.location.hostname);
console.log('Port:', window.location.port);
console.log('Protocol:', window.location.protocol);

class EventoAPI {
    static getAuthHeaders() {
        const token = localStorage.getItem('jwtToken');
        return token ? { 'Authorization': `Bearer ${token}` } : {};
    }
    static async getEventos() {
        try {
            console.log('Obteniendo eventos desde:', `${API_BASE_URL}/Eventos`);
            const response = await fetch(`${API_BASE_URL}/Eventos`);
            if (!response.ok) {
                const errorText = await response.text();
                console.error('Error del servidor:', errorText);
                throw new Error(`Error al obtener eventos: ${response.status} - ${errorText}`);
            }
            const data = await response.json();
            console.log('Eventos obtenidos:', data.length);
            return data;
        } catch (error) {
            console.error('Error al obtener eventos:', error);
            throw error;
        }
    }

    static async getEventosUsuario() {
        const response = await fetch(`${API_BASE_URL}/Eventos/usuario`, {
            headers: {
                ...this.getAuthHeaders(),
            },
        });
        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Error al obtener eventos del usuario: ${errorText || response.statusText}`);
        }
        return await response.json();
    }

    static async getEvento(id) {
        const response = await fetch(`${API_BASE_URL}/Eventos/${id}`);
        if (!response.ok) {
            throw new Error('Error al obtener evento');
        }
        return await response.json();
    }

    static async createEvento(evento) {
        const response = await fetch(`${API_BASE_URL}/Eventos`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                ...this.getAuthHeaders(),
            },
            body: JSON.stringify(evento),
        });
        if (!response.ok) {
            const error = await response.text();
            throw new Error(`Error al crear evento: ${error}`);
        }
        return await response.json();
    }

    static async updateEvento(id, evento) {
        const response = await fetch(`${API_BASE_URL}/Eventos/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                ...this.getAuthHeaders(),
            },
            body: JSON.stringify(evento),
        });
        if (!response.ok) {
            const error = await response.text();
            throw new Error(`Error al actualizar evento: ${error}`);
        }
    }

    static async deleteEvento(id) {
        const response = await fetch(`${API_BASE_URL}/Eventos/${id}`, {
            method: 'DELETE',
            headers: this.getAuthHeaders(),
        });
        if (!response.ok) {
            throw new Error('Error al eliminar evento');
        }
    }

    static async getTipoEventos() {
        const response = await fetch(`${API_BASE_URL}/TipoEventos`);
        if (!response.ok) {
            throw new Error('Error al obtener tipos de evento');
        }
        return await response.json();
    }

    static async getPersonas() {
        const response = await fetch(`${API_BASE_URL}/Personas`);
        if (!response.ok) {
            throw new Error('Error al obtener personas');
        }
        return await response.json();
    }

    static async getPersona(id) {
        const response = await fetch(`${API_BASE_URL}/Personas/${id}`);
        if (!response.ok) {
            throw new Error('Error al obtener persona');
        }
        return await response.json();
    }

    static async createPersona(persona) {
        const response = await fetch(`${API_BASE_URL}/Personas`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                ...this.getAuthHeaders(),
            },
            body: JSON.stringify(persona),
        });
        if (!response.ok) {
            const error = await response.text();
            throw new Error(`Error al crear persona: ${error}`);
        }
        return await response.json();
    }

    static async updatePersona(id, persona) {
        const response = await fetch(`${API_BASE_URL}/Personas/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                ...this.getAuthHeaders(),
            },
            body: JSON.stringify(persona),
        });
        if (!response.ok) {
            const error = await response.text();
            throw new Error(`Error al actualizar persona: ${error}`);
        }
    }

    static async deletePersona(id) {
        const response = await fetch(`${API_BASE_URL}/Personas/${id}`, {
            method: 'DELETE',
            headers: this.getAuthHeaders(),
        });
        if (!response.ok) {
            const error = await response.text();
            throw new Error(`Error al eliminar persona: ${response.statusText}`);
        }
    }

    static async getTipoEvento(id) {
        const response = await fetch(`${API_BASE_URL}/TipoEventos/${id}`);
        if (!response.ok) {
            throw new Error('Error al obtener tipo de evento');
        }
        return await response.json();
    }

    static async createTipoEvento(tipoEvento) {
        const response = await fetch(`${API_BASE_URL}/TipoEventos`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                ...this.getAuthHeaders(),
            },
            body: JSON.stringify(tipoEvento),
        });
        if (!response.ok) {
            const error = await response.text();
            throw new Error(`Error al crear tipo de evento: ${error}`);
        }
        return await response.json();
    }

    static async updateTipoEvento(id, tipoEvento) {
        const response = await fetch(`${API_BASE_URL}/TipoEventos/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                ...this.getAuthHeaders(),
            },
            body: JSON.stringify(tipoEvento),
        });
        if (!response.ok) {
            const error = await response.text();
            throw new Error(`Error al actualizar tipo de evento: ${error}`);
        }
    }

    static async deleteTipoEvento(id) {
        const response = await fetch(`${API_BASE_URL}/TipoEventos/${id}`, {
            method: 'DELETE',
            headers: this.getAuthHeaders(),
        });
        if (!response.ok) {
            const error = await response.text();
            throw new Error(`Error al eliminar tipo de evento: ${response.statusText}`);
        }
    }

    static async getRegistroAsistencias() {
        const response = await fetch(`${API_BASE_URL}/RegistroAsistencias`);
        if (!response.ok) {
            throw new Error('Error al obtener asistencias');
        }
        return await response.json();
    }

    static async getRegistroAsistencia(id) {
        const response = await fetch(`${API_BASE_URL}/RegistroAsistencias/${id}`);
        if (!response.ok) {
            throw new Error('Error al obtener asistencia');
        }
        return await response.json();
    }

    static async createRegistroAsistencia(asistencia) {
        const response = await fetch(`${API_BASE_URL}/RegistroAsistencias`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                ...this.getAuthHeaders(),
            },
            body: JSON.stringify(asistencia),
        });
        if (!response.ok) {
            const error = await response.text();
            throw new Error(`Error al crear asistencia: ${error}`);
        }
        return await response.json();
    }

    static async updateRegistroAsistencia(id, asistencia) {
        const response = await fetch(`${API_BASE_URL}/RegistroAsistencias/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                ...this.getAuthHeaders(),
            },
            body: JSON.stringify(asistencia),
        });
        if (!response.ok) {
            const error = await response.text();
            throw new Error(`Error al actualizar asistencia: ${error}`);
        }
    }

    static async deleteRegistroAsistencia(id) {
        const response = await fetch(`${API_BASE_URL}/RegistroAsistencias/${id}`, {
            method: 'DELETE',
            headers: this.getAuthHeaders(),
        });
        if (!response.ok) {
            const error = await response.text();
            throw new Error(`Error al eliminar asistencia: ${response.statusText}`);
        }
    }

    static async login(credentials) {
        const response = await fetch(`${API_BASE_URL}/Usuarios/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(credentials),
        });
        if (!response.ok) {
            throw new Error('Credenciales inválidas');
        }
        return await response.json();
    }

    static async register(userData) {
        const response = await fetch(`${API_BASE_URL}/Usuarios/register`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(userData),
        });
        if (!response.ok) {
            const error = await response.text();
            throw new Error(`Error al registrar: ${error}`);
        }
        return await response.json();
    }
}

export default EventoAPI;