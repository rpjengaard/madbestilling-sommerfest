import { LitElement, html, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';

const API = '/umbraco/api/madbestilling/orders';

// [CHANGE: new status set] Related: Controllers/OrdersApiController.cs, Models/OrderRecord.cs
const STATUS_LABELS = { 'ny': 'Ny order', 'order-betalt': 'Order betalt', 'problem': 'Problem' };
const STATUS_COLORS = { 'ny': '#f59e0b', 'order-betalt': '#16a34a', 'problem': '#dc2626' };

class OrdersDashboard extends UmbElementMixin(LitElement) {
    static properties = {
        _orders:        { state: true },
        _loading:       { state: true },
        _error:         { state: true },
        _selected:      { state: true },
        _editMode:      { state: true },
        _editFields:    { state: true },
        _saving:        { state: true },
        _confirmDelete: { state: true },
    };

    static styles = css`
        :host { display: block; padding: 28px; font-family: var(--uui-font-family, Arial, sans-serif); }

        h1 { font-size: 1.5rem; font-weight: 900; color: #2d4b8a; margin: 0 0 20px; }

        .toolbar { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
        .toolbar h1 { margin: 0; }
        .btn-export { background: #16a34a; color: #fff; font-weight: 700; font-size: .85rem; padding: 10px 22px; border: none; border-radius: 100px; cursor: pointer; display: inline-flex; align-items: center; gap: 8px; transition: background .15s; }
        .btn-export:hover { background: #15803d; }

        .error { color: #dc2626; padding: 12px; background: #fef2f2; border-radius: 8px; font-size: .85rem; }

        table { width: 100%; border-collapse: collapse; background: #fff; border-radius: 12px; overflow: hidden; box-shadow: 0 1px 6px rgba(0,0,0,.07); }
        th { background: #2d4b8a; color: #fff; text-align: left; padding: 12px 16px; font-size: .72rem; text-transform: uppercase; letter-spacing: .07em; white-space: nowrap; }
        td { padding: 12px 16px; font-size: .85rem; border-bottom: 1px solid #f0f4fb; }
        tbody tr { cursor: pointer; transition: background .12s; }
        tbody tr:hover { background: #f0f4fb; }
        tbody tr:last-child td { border-bottom: none; }

        .badge { display: inline-block; padding: 3px 10px; border-radius: 100px; font-size: .68rem; font-weight: 700; color: #fff; }

        /* Slide-in panel */
        .overlay { position: fixed; inset: 0; background: rgba(0,0,0,.3); z-index: 1000; display: flex; justify-content: flex-end; }
        .panel { width: 500px; max-width: 100vw; height: 100vh; background: #fff; overflow-y: auto; padding: 32px; box-shadow: -6px 0 24px rgba(0,0,0,.12); display: flex; flex-direction: column; gap: 20px; }
        .panel-header { display: flex; justify-content: space-between; align-items: center; }
        .panel-header h2 { font-size: 1.15rem; font-weight: 900; color: #2d4b8a; margin: 0; }
        .btn-close { background: none; border: none; font-size: 1.6rem; cursor: pointer; color: #aaa; line-height: 1; padding: 0; }
        .btn-close:hover { color: #333; }

        .detail-grid { display: grid; grid-template-columns: 90px 1fr; gap: 6px 12px; font-size: .875rem; }
        .detail-grid .lbl { color: #888; }
        .detail-grid .val { font-weight: 600; color: #1a1a2e; }

        .items-table { width: 100%; border-collapse: collapse; }
        .items-table th { background: #eef0f4; color: #2d4b8a; font-size: .7rem; text-transform: uppercase; padding: 8px 10px; text-align: left; }
        .items-table td { padding: 8px 10px; font-size: .82rem; border-bottom: 1px solid #f0f4fb; }
        .items-table tr:last-child td { border-bottom: none; }

        label.field-label { font-size: .75rem; font-weight: 700; color: #2d4b8a; text-transform: uppercase; letter-spacing: .06em; display: block; margin-bottom: 6px; }
        select, input.field-input, textarea.field-input { width: 100%; padding: 10px 14px; border: 1px solid #ddd; border-radius: 8px; font-size: .875rem; box-sizing: border-box; font-family: inherit; resize: vertical; }
        input.field-input { margin-top: 0; }

        .btn-save { background: #2d4b8a; color: #fff; font-weight: 700; font-size: .875rem; padding: 12px 28px; border: none; border-radius: 100px; cursor: pointer; width: 100%; transition: background .15s; }
        .btn-save:hover { background: #1e3870; }
        .btn-save:disabled { opacity: .5; cursor: not-allowed; }

        .btn-edit { background: #f0f4fb; color: #2d4b8a; font-weight: 700; font-size: .875rem; padding: 10px 20px; border: none; border-radius: 100px; cursor: pointer; transition: background .15s; }
        .btn-edit:hover { background: #dce4f5; }

        .btn-delete { background: #fef2f2; color: #dc2626; font-weight: 700; font-size: .875rem; padding: 10px 20px; border: none; border-radius: 100px; cursor: pointer; transition: background .15s; }
        .btn-delete:hover { background: #fee2e2; }
        .btn-delete:disabled { opacity: .5; cursor: not-allowed; }

        .btn-cancel { background: none; color: #888; font-weight: 600; font-size: .875rem; padding: 10px 20px; border: 1px solid #ddd; border-radius: 100px; cursor: pointer; }
        .btn-cancel:hover { background: #f5f5f5; }

        .action-row { display: flex; gap: 8px; }
        .action-row .btn-save { flex: 1; }

        .confirm-box { background: #fef2f2; border: 1px solid #fca5a5; border-radius: 12px; padding: 16px; display: flex; flex-direction: column; gap: 10px; }
        .confirm-box p { margin: 0; font-size: .875rem; color: #7f1d1d; font-weight: 600; }
        .confirm-box .confirm-actions { display: flex; gap: 8px; }

        .section-title { font-size: .72rem; font-weight: 700; color: #2d4b8a; text-transform: uppercase; letter-spacing: .07em; border-bottom: 1px solid #e5e7eb; padding-bottom: 6px; }

        .field-group { display: flex; flex-direction: column; gap: 6px; }
    `;

    constructor() {
        super();
        this._orders        = [];
        this._loading       = true;
        this._error         = '';
        this._selected      = null;
        this._editMode      = false;
        this._editFields    = {};
        this._saving        = false;
        this._confirmDelete = false;
    }

    connectedCallback() {
        super.connectedCallback();
        this._loadOrders().then(() => {
            // [CHANGE: open order from ?orderId= in either location.search or hash] Related: code/Services/OrderEmailService.cs
            const searchParams = new URLSearchParams(window.location.search);
            const hash         = window.location.hash;
            const hashParams   = new URLSearchParams(hash.includes('?') ? hash.slice(hash.indexOf('?') + 1) : '');
            const id           = parseInt(searchParams.get('orderId') ?? hashParams.get('orderId') ?? '');
            if (id) {
                const order = this._orders.find(o => o.id === id);
                if (order) this._open(order);
            }
        });
    }

    async _loadOrders() {
        this._loading = true;
        this._error   = '';
        try {
            const res = await fetch(`${API}/GetAllOrders`, { credentials: 'include' });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            this._orders = await res.json();
        } catch (e) {
            this._error = `Kunne ikke hente bestillinger: ${e.message}`;
        } finally {
            this._loading = false;
        }
    }

    _open(order) {
        this._selected      = order;
        this._editMode      = false;
        this._confirmDelete = false;
        this._editFields    = { childName: order.childName, childClass: order.childClass, phone: order.phone, email: order.email, status: order.status, note: order.note ?? '' };
    }

    _close() {
        this._selected      = null;
        this._editMode      = false;
        this._confirmDelete = false;
    }

    _startEdit() {
        this._editMode      = true;
        this._confirmDelete = false;
    }

    _cancelEdit() { this._editMode = false; }

    _field(key, value) {
        this._editFields = { ...this._editFields, [key]: value };
    }

    async _saveOrder() {
        if (!this._selected) return;
        this._saving = true;
        try {
            const res = await fetch(`${API}/UpdateOrder/${this._selected.id}`, {
                method: 'PUT',
                credentials: 'include',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(this._editFields),
            });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const updated = await res.json();
            this._orders   = this._orders.map(o => o.id === updated.id ? updated : o);
            this._selected = updated;
            this._editMode = false;
        } catch (e) {
            this._error = `Gem fejlede: ${e.message}`;
        } finally {
            this._saving = false;
        }
    }

    async _deleteOrder() {
        if (!this._selected) return;
        this._saving = true;
        try {
            const res = await fetch(`${API}/DeleteOrder/${this._selected.id}`, {
                method: 'DELETE',
                credentials: 'include',
            });
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            this._orders = this._orders.filter(o => o.id !== this._selected.id);
            this._close();
        } catch (e) {
            this._error = `Sletning fejlede: ${e.message}`;
        } finally {
            this._saving = false;
        }
    }

    _fmt(dateStr) {
        const d = new Date(dateStr);
        return d.toLocaleDateString('da-DK') + ' ' + d.toLocaleTimeString('da-DK', { hour: '2-digit', minute: '2-digit' });
    }

    _dkk(n) { return Number(n).toLocaleString('da-DK', { minimumFractionDigits: 2, maximumFractionDigits: 2 }); }

    _cart(json) { try { return JSON.parse(json) || []; } catch { return []; } }

    _exportExcel() {
        window.location.href = `${API}/ExportOrders`;
    }

    render() {
        return html`
            <div class="toolbar">
                <h1>Bestillinger</h1>
                <button class="btn-export" @click=${this._exportExcel} title="Download alle bestillinger som Excel">
                    📊 Download Excel
                </button>
            </div>

            ${this._error ? html`<p class="error">${this._error}</p>` : ''}

            ${this._loading
                ? html`<p>Indlæser...</p>`
                : html`
                <table>
                    <thead>
                        <tr>
                            <th>#</th>
                            <th>Barn</th>
                            <th>Klasse</th>
                            <th>Total</th>
                            <th>Status</th>
                            <th>Tidspunkt</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${this._orders.map(o => html`
                            <tr @click=${() => this._open(o)}>
                                <td>${o.id}</td>
                                <td>${o.childName}</td>
                                <td>${o.childClass}</td>
                                <td>${this._dkk(o.totalAmount)} kr.</td>
                                <td>
                                    <span class="badge" style="background:${STATUS_COLORS[o.status] ?? '#999'}">
                                        ${STATUS_LABELS[o.status] ?? o.status}
                                    </span>
                                </td>
                                <td>${this._fmt(o.createdAt)}</td>
                            </tr>
                        `)}
                    </tbody>
                </table>
            `}

            ${this._selected ? html`
                <div class="overlay" @click=${e => e.target === e.currentTarget && this._close()}>
                    <div class="panel">
                        <div class="panel-header">
                            <h2>Bestilling #${this._selected.id}</h2>
                            <button class="btn-close" @click=${this._close}>×</button>
                        </div>

                        ${this._editMode ? html`

                            <div>
                                <p class="section-title">Rediger kundeoplysninger</p>
                                <div style="display:flex;flex-direction:column;gap:14px;margin-top:14px;">
                                    <div class="field-group">
                                        <label class="field-label">Barnets navn</label>
                                        <input class="field-input" type="text" .value=${this._editFields.childName}
                                               @input=${e => this._field('childName', e.target.value)} />
                                    </div>
                                    <div class="field-group">
                                        <label class="field-label">Klasse</label>
                                        <input class="field-input" type="text" .value=${this._editFields.childClass}
                                               @input=${e => this._field('childClass', e.target.value)} />
                                    </div>
                                    <div class="field-group">
                                        <label class="field-label">Mobil</label>
                                        <input class="field-input" type="text" .value=${this._editFields.phone}
                                               @input=${e => this._field('phone', e.target.value)} />
                                    </div>
                                    <div class="field-group">
                                        <label class="field-label">E-mail</label>
                                        <input class="field-input" type="email" .value=${this._editFields.email}
                                               @input=${e => this._field('email', e.target.value)} />
                                    </div>
                                    <div class="field-group">
                                        <label class="field-label">Status</label>
                                        <select .value=${this._editFields.status} @change=${e => this._field('status', e.target.value)}>
                                            <option value="ny">Ny order</option>
                                            <option value="order-betalt">Order betalt</option>
                                            <option value="problem">Problem</option>
                                        </select>
                                    </div>
                                    ${this._editFields.status === 'problem' ? html`
                                        <div class="field-group">
                                            <label class="field-label">Beskrivelse af problem</label>
                                            <textarea class="field-input" rows="4"
                                                      .value=${this._editFields.note}
                                                      @input=${e => this._field('note', e.target.value)}
                                                      placeholder="Beskriv hvad der er galt med ordren eller betalingen..."></textarea>
                                        </div>
                                    ` : ''}
                                </div>
                            </div>

                            <div class="action-row">
                                <button class="btn-cancel" @click=${this._cancelEdit}>Annuller</button>
                                <button class="btn-save" @click=${this._saveOrder} ?disabled=${this._saving}>
                                    ${this._saving ? 'Gemmer...' : 'Gem ændringer'}
                                </button>
                            </div>

                        ` : html`

                            <div>
                                <p class="section-title">Kundeoplysninger</p>
                                <div class="detail-grid" style="margin-top:12px;">
                                    <span class="lbl">Barn</span>      <span class="val">${this._selected.childName}</span>
                                    <span class="lbl">Klasse</span>    <span class="val">${this._selected.childClass}</span>
                                    <span class="lbl">Mobil</span>     <span class="val">${this._selected.phone}</span>
                                    <span class="lbl">E-mail</span>    <span class="val">${this._selected.email}</span>
                                    <span class="lbl">Tidspunkt</span> <span class="val">${this._fmt(this._selected.createdAt)}</span>
                                    <span class="lbl">Total</span>     <span class="val">${this._dkk(this._selected.totalAmount)} kr.</span>
                                    <span class="lbl">Status</span>
                                    <span class="val">
                                        <span class="badge" style="background:${STATUS_COLORS[this._selected.status] ?? '#999'}">
                                            ${STATUS_LABELS[this._selected.status] ?? this._selected.status}
                                        </span>
                                    </span>
                                </div>
                            </div>

                            ${this._selected.status === 'problem' && this._selected.note ? html`
                                <div class="confirm-box">
                                    <p style="color:#7f1d1d;font-weight:700;">Problem beskrivelse</p>
                                    <p style="margin:0;font-size:.85rem;color:#7f1d1d;font-weight:500;white-space:pre-wrap;">${this._selected.note}</p>
                                </div>
                            ` : ''}

                            <div>
                                <p class="section-title">Bestilte retter</p>
                                <table class="items-table" style="margin-top:12px;">
                                    <thead><tr><th>Ret</th><th>Antal</th><th>Stk.</th><th>Total</th></tr></thead>
                                    <tbody>
                                        ${this._cart(this._selected.cartJson).map(item => html`
                                            <tr>
                                                <td>${item.name}</td>
                                                <td>${item.qty}</td>
                                                <td>${this._dkk(item.price)} kr.</td>
                                                <td>${this._dkk(item.price * item.qty)} kr.</td>
                                            </tr>
                                        `)}
                                    </tbody>
                                </table>
                            </div>

                            <div class="action-row">
                                <button class="btn-edit" @click=${this._startEdit}>Rediger</button>
                                <button class="btn-delete" @click=${() => this._confirmDelete = true} ?disabled=${this._saving}>Slet ordre</button>
                            </div>

                            ${this._confirmDelete ? html`
                                <div class="confirm-box">
                                    <p>Er du sikker på, at du vil slette bestilling #${this._selected.id}? Dette kan ikke fortrydes.</p>
                                    <div class="confirm-actions">
                                        <button class="btn-cancel" @click=${() => this._confirmDelete = false}>Annuller</button>
                                        <button class="btn-delete" @click=${this._deleteOrder} ?disabled=${this._saving}>
                                            ${this._saving ? 'Sletter...' : 'Ja, slet'}
                                        </button>
                                    </div>
                                </div>
                            ` : ''}

                        `}
                    </div>
                </div>
            ` : ''}
        `;
    }
}

customElements.define('orders-dashboard', OrdersDashboard);
