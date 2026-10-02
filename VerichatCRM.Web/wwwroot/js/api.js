const API_BASE = window.VERICHAT_API_URL || 'http://verichat-crm-api.runasp.net';

const api = {
    async get(endpoint) {
        const token = localStorage.getItem('jwt');
        const res = await fetch(`${API_BASE}${endpoint}`, {
            headers: {
                'Authorization': token ? `Bearer ${token}` : '',
                'Content-Type': 'application/json'
            }
        });
        if (!res.ok) throw new Error(`API error: ${res.status}`);
        return res.json();
    },

    async post(endpoint, body) {
        const token = localStorage.getItem('jwt');
        const res = await fetch(`${API_BASE}${endpoint}`, {
            method: 'POST',
            headers: {
                'Authorization': token ? `Bearer ${token}` : '',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(body)
        });
        if (!res.ok) throw new Error(`API error: ${res.status}`);
        return res.json();
    }
};