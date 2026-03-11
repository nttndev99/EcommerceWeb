// MVC/wwwroot/js/admin-table.js
// ✅ Shared AJAX table component — dùng chung cho Category, Product, Customer, Order

class AdminTable {
    constructor(config) {
        this.endpoint   = config.endpoint;      // '/Admin/Category/GetFiltered'
        this.tableBody  = config.tableBody;     // 'categoryTableBody'
        this.pagination = config.pagination;    // 'categoryPagination'
        this.renderRow  = config.renderRow;     // function(item, stt) => html string
        this.colSpan    = config.colSpan || 6;
        this.pageSize   = config.pageSize || 8;
        this.varName    = config.varName || 'adminTable'; // ✅ tên biến JS global để dùng trong onclick

        this.state = {
            search: '', status: config.defaultStatus || 'active',
            sortBy: config.defaultSort || 'id', sortDir: 'asc',
            fromDate: '', toDate: '', page: 1,
            ...config.extraState
        };

        this._debounceTimer = null;
        this._token = document.querySelector('[name="__RequestVerificationToken"]')?.value;
    }

    // ── Load data ────────────────────────────────────────
    async load() {
        const params = new URLSearchParams(
            Object.fromEntries(Object.entries(this.state).filter(([_, v]) => v !== '' && v !== null))
        );
        try {
            const res  = await fetch(`${this.endpoint}?${params}`);
            const data = await res.json();
            this._renderTable(data.items, data.currentPage, this.pageSize);
            this._renderPagination(data.currentPage, data.totalPages);
        } catch {
            this._setBody(`<tr><td colspan="${this.colSpan}" class="text-center text-danger py-3">Error loading data.</td></tr>`);
        }
    }

    // ── Render table ─────────────────────────────────────
    _renderTable(items, page, size) {
        if (!items?.length) {
            this._setBody(`<tr><td colspan="${this.colSpan}" class="text-center text-muted py-4">No data available.</td></tr>`);
            return;
        }
        let stt = (page - 1) * size + 1;
        document.getElementById(this.tableBody).innerHTML =
            items.map(item => this.renderRow(item, stt++)).join('');
    }

    _setBody(html) {
        document.getElementById(this.tableBody).innerHTML = html;
    }

    // ── Render pagination ─────────────────────────────────
    _renderPagination(cur, total) {
        const el = document.getElementById(this.pagination);
        if (!el) return;
        if (total <= 1) { el.innerHTML = ''; return; }

        // ✅ Dùng this.varName thay vì hardcode 'adminTable'
        const v = this.varName;
        const btn = (label, pg, disabled = false, active = false) =>
            `<button class="btn btn-sm ${active ? 'btn-primary' : 'btn-outline-secondary'}"
                ${disabled ? 'disabled' : `onclick="${v}.goPage(${pg})"`}>${label}</button>`;

        const start = Math.max(1, cur - 2);
        const end   = Math.min(total, start + 4);
        let   html  = btn('«', 1, cur === 1) + btn('‹', cur - 1, cur === 1);
        for (let i = start; i <= end; i++) html += btn(i, i, false, i === cur);
        html += btn('›', cur + 1, cur === total) + btn('»', total, cur === total);
        html += `<span class="text-muted small ms-2">Page ${cur}/${total}</span>`;
        el.innerHTML = html;
    }

    goPage(p) { this.state.page = p; this.load(); }

    // ── AJAX POST actions ─────────────────────────────────
    async post(url, body = {}) {
        const formBody = Object.entries(body).map(([k,v]) => `${k}=${v}`).join('&');
        const res = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': this._token
            },
            body: formBody
        });
        return res.json();
    }

    async softDelete(id, name) {
        if (!confirm(`Delete "${name}"?`)) return;
        const r = await this.post(`/Admin/${this._ctrl()}/Delete`, { id });
        this._toast(r.message, r.success);
        if (r.success) this.load();
    }

    async restore(id) {
        if (!confirm('Restore this item?')) return;
        const r = await this.post(`/Admin/${this._ctrl()}/Restore`, { id });
        this._toast(r.message, r.success);
        if (r.success) this.load();
    }

    async hardDelete(id, name) {
        if (!confirm(`Delete "${name}" permanently?\nThis action cannot be undone!`)) return;
        const r = await this.post(`/Admin/${this._ctrl()}/HardDelete`, { id });
        this._toast(r.message, r.success);
        if (r.success) this.load();
    }

    _ctrl() {
        return this.endpoint.split('/')[2]; // '/Admin/Category/GetFiltered' → 'Category'
    }

    // ── Toast ─────────────────────────────────────────────
    _toast(msg, ok = true) {
        const id  = `t${Date.now()}`;
        const div = document.createElement('div');
        div.id        = id;
        div.className = `toast align-items-center text-bg-${ok ? 'success' : 'danger'} border-0 show mb-2`;
        div.innerHTML = `<div class="d-flex">
            <div class="toast-body">${msg}</div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto"
                onclick="document.getElementById('${id}').remove()"></button>
        </div>`;
        let container = document.getElementById('toastContainer');
        if (!container) {
            container = document.createElement('div');
            container.id        = 'toastContainer';
            container.className = 'position-fixed top-0 end-0 p-3';
            container.style.zIndex = '9999';
            document.body.append(container);
        }
        container.append(div);
        setTimeout(() => div.remove(), 3000);
    }

    // ── Bind filter controls ──────────────────────────────
    bindSearch(elId) {
        document.getElementById(elId)?.addEventListener('input', e => {
            clearTimeout(this._debounceTimer);
            this._debounceTimer = setTimeout(() => {
                this.state.search = e.target.value;
                this.state.page = 1; this.load();
            }, 400);
        });
    }

    bindSelect(elId, stateKey) {
        document.getElementById(elId)?.addEventListener('change', e => {
            this.state[stateKey] = e.target.value;
            this.state.page = 1; this.load();
        });
    }

    bindDate(elId, stateKey) {
        document.getElementById(elId)?.addEventListener('change', e => {
            this.state[stateKey] = e.target.value;
            this.state.page = 1; this.load();
        });
    }

    bindSort(elId) {
        document.getElementById(elId)?.addEventListener('change', e => {
            const [by, dir] = e.target.value.split('|');
            this.state.sortBy = by; this.state.sortDir = dir;
            this.state.page = 1; this.load();
        });
    }

    bindReset(elId, defaults = {}) {
        document.getElementById(elId)?.addEventListener('click', () => {
            Object.assign(this.state, {
                search: '', status: 'active', fromDate: '', toDate: '',
                sortBy: 'id', sortDir: 'asc', page: 1, ...defaults
            });
            // Clear UI inputs
            ['search','status','fromDate','toDate','sort'].forEach(id => {
                const el = document.getElementById(id);
                if (el) el.value = el.tagName === 'SELECT' && id === 'status' ? 'active'
                                : el.tagName === 'SELECT' && id === 'sort'   ? 'id|asc'
                                : '';
            });
            this.load();
        });
    }
}

// ── Helpers ────────────────────────────────────────────
const esc     = s => s?.replace(/[&<>"]/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;'}[c])) ?? '';
const fmtDate = s => s ? new Date(s).toLocaleDateString('vi-VN') : '';
const fmtCurr = n => new Intl.NumberFormat('vi-VN', { style:'currency', currency:'VND' }).format(n);

const statusBadge = isDeleted =>
    isDeleted ? '<span class="badge bg-danger">Deleted</span>'
            : '<span class="badge bg-success">Active</span>';

const actionBtns = (item, idKey, nameKey, ctrl) =>
    item.isDeleted ? `
        <button class="btn btn-success btn-sm" onclick="${ctrl}.restore(${item[idKey]})">↩ Restore</button>
        <button class="btn btn-danger btn-sm"  onclick="${ctrl}.hardDelete(${item[idKey]},'${esc(item[nameKey])}')">✖ Delete Permanently</button>
    ` : `
        <a href="/Admin/${ctrl.constructor.name.replace('Controller','')}/Edit/${item[idKey]}" class="btn btn-warning btn-sm">✏ Edit</a>
        <button class="btn btn-danger btn-sm" onclick="${ctrl}.softDelete(${item[idKey]},'${esc(item[nameKey])}')">🗑 Delete</button>
    `;