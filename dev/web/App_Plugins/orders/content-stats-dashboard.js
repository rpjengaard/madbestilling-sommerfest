// [CHANGE: new statistik-dashboard til content-sektionen, erstatter Umb.Dashboard.Welcome] Related: umbraco-package.json, entry-point.js
import { LitElement, html, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';

const API = '/umbraco/api/madbestilling/orders';
const STATUS_LABELS = { 'ny': 'Ny order', 'order-betalt': 'Order betalt', 'problem': 'Problem' };
const STATUS_COLORS = { 'ny': '#f59e0b', 'order-betalt': '#16a34a', 'problem': '#dc2626' };

class ContentStatsDashboard extends UmbElementMixin(LitElement) {
    static properties = {
        _orders:  { state: true },
        _loading: { state: true },
        _error:   { state: true },
    };

    static styles = css`
        :host { display: block; padding: 28px; font-family: var(--uui-font-family, Arial, sans-serif); color: #1a1a2e; }

        .header { display: flex; justify-content: space-between; align-items: flex-start; gap: 16px; margin-bottom: 24px; flex-wrap: wrap; }
        .header h1 { font-size: 1.65rem; font-weight: 900; color: #2d4b8a; margin: 0 0 4px; }
        .header .subtitle { font-size: .9rem; color: #6b7280; margin: 0; }

        .btn-primary { background: #2d4b8a; color: #fff; font-weight: 700; font-size: .85rem; padding: 12px 24px; border: none; border-radius: 100px; cursor: pointer; display: inline-flex; align-items: center; gap: 8px; transition: background .15s; text-decoration: none; }
        .btn-primary:hover { background: #1e3870; }

        .error { color: #dc2626; padding: 12px; background: #fef2f2; border-radius: 8px; font-size: .85rem; }

        .kpi-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 16px; margin-bottom: 28px; }
        .kpi-card { background: #fff; border-radius: 12px; padding: 18px 22px; box-shadow: 0 1px 6px rgba(0,0,0,.07); display: flex; flex-direction: column; gap: 6px; border-left: 4px solid #2d4b8a; }
        .kpi-card.paid    { border-left-color: #16a34a; }
        .kpi-card.pending { border-left-color: #f59e0b; }
        .kpi-card.problem { border-left-color: #dc2626; }
        .kpi-card.neutral { border-left-color: #6b7280; }
        .kpi-label { font-size: .7rem; font-weight: 700; color: #6b7280; text-transform: uppercase; letter-spacing: .07em; }
        .kpi-value { font-size: 1.6rem; font-weight: 900; color: #2d4b8a; line-height: 1.1; }
        .kpi-meta  { font-size: .72rem; color: #9ca3af; font-weight: 600; }

        .row { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; margin-bottom: 20px; }
        @media (max-width: 1100px) { .row { grid-template-columns: 1fr; } }

        .card { background: #fff; border-radius: 12px; padding: 20px 24px; box-shadow: 0 1px 6px rgba(0,0,0,.07); }
        .card h2 { font-size: .85rem; font-weight: 800; color: #2d4b8a; text-transform: uppercase; letter-spacing: .08em; margin: 0 0 14px; }

        table { width: 100%; border-collapse: collapse; }
        th { background: transparent; color: #6b7280; text-align: left; padding: 8px 10px; font-size: .68rem; text-transform: uppercase; letter-spacing: .06em; border-bottom: 1px solid #e5e7eb; }
        td { padding: 10px; font-size: .85rem; border-bottom: 1px solid #f0f4fb; }
        tbody tr:last-child td { border-bottom: none; }

        .badge { display: inline-block; padding: 3px 10px; border-radius: 100px; font-size: .68rem; font-weight: 700; color: #fff; }

        .empty { font-size: .85rem; color: #9ca3af; font-style: italic; margin: 8px 0 0; }
    `;

    constructor() {
        super();
        this._orders  = [];
        this._loading = true;
        this._error   = '';
    }

    connectedCallback() {
        super.connectedCallback();
        this._load();
    }

    async _load() {
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

    _dkk(n) { return Number(n).toLocaleString('da-DK', { minimumFractionDigits: 2, maximumFractionDigits: 2 }); }
    _fmt(d) {
        const x = new Date(d);
        return x.toLocaleDateString('da-DK') + ' ' + x.toLocaleTimeString('da-DK', { hour: '2-digit', minute: '2-digit' });
    }
    _cart(json) { try { return JSON.parse(json) || []; } catch { return []; } }

    _goToOrders() {
        window.location.href = '/umbraco/section/bestillinger';
    }

    render() {
        if (this._loading) return html`<p>Indlæser statistik...</p>`;

        const all      = this._orders;
        const paid     = all.filter(o => o.status === 'order-betalt');
        const nyOrder  = all.filter(o => o.status === 'ny');
        const problem  = all.filter(o => o.status === 'problem');
        const sum      = l => l.reduce((s, o) => s + Number(o.totalAmount || 0), 0);
        const totalAll = sum(all);
        const totalPaid = sum(paid);
        const avgOrder  = all.length ? totalAll / all.length : 0;
        const paidRate  = all.length ? (paid.length / all.length * 100) : 0;

        const itemMap = new Map();
        all.forEach(o => {
            this._cart(o.cartJson).forEach(item => {
                const cur = itemMap.get(item.name) || { qty: 0, revenue: 0 };
                cur.qty     += Number(item.qty || 0);
                cur.revenue += Number(item.price || 0) * Number(item.qty || 0);
                itemMap.set(item.name, cur);
            });
        });
        const topItems = [...itemMap.entries()].sort((a, b) => b[1].qty - a[1].qty).slice(0, 8);

        const classMap = new Map();
        all.forEach(o => {
            const k = (o.childClass || 'Ukendt').trim();
            const cur = classMap.get(k) || { count: 0, revenue: 0 };
            cur.count++;
            cur.revenue += Number(o.totalAmount || 0);
            classMap.set(k, cur);
        });
        const byClass = [...classMap.entries()].sort((a, b) => b[1].revenue - a[1].revenue);

        const recent = [...all].sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt)).slice(0, 8);

        return html`
            <div class="header">
                <div>
                    <h1>Madbestilling – Statistik</h1>
                    <p class="subtitle">Overblik over alle bestillinger til sommerfesten</p>
                </div>
                <button class="btn-primary" @click=${this._goToOrders}>
                    Gå til bestillinger →
                </button>
            </div>

            ${this._error ? html`<p class="error">${this._error}</p>` : ''}

            <div class="kpi-grid">
                <div class="kpi-card">
                    <span class="kpi-label">Total omsætning</span>
                    <span class="kpi-value">${this._dkk(totalAll)} kr.</span>
                    <span class="kpi-meta">${all.length} ${all.length === 1 ? 'bestilling' : 'bestillinger'} i alt</span>
                </div>
                <div class="kpi-card paid">
                    <span class="kpi-label">Order betalt</span>
                    <span class="kpi-value">${this._dkk(totalPaid)} kr.</span>
                    <span class="kpi-meta">${paid.length} ordrer · ${paidRate.toFixed(0)}% betalingsrate</span>
                </div>
                <div class="kpi-card pending">
                    <span class="kpi-label">Afventer betaling</span>
                    <span class="kpi-value">${this._dkk(sum(nyOrder))} kr.</span>
                    <span class="kpi-meta">${nyOrder.length} ordrer i kø</span>
                </div>
                <div class="kpi-card problem">
                    <span class="kpi-label">Med problemer</span>
                    <span class="kpi-value">${this._dkk(sum(problem))} kr.</span>
                    <span class="kpi-meta">${problem.length} ordrer kræver opmærksomhed</span>
                </div>
                <div class="kpi-card neutral">
                    <span class="kpi-label">Gennemsnit pr. ordre</span>
                    <span class="kpi-value">${this._dkk(avgOrder)} kr.</span>
                    <span class="kpi-meta">på tværs af alle bestillinger</span>
                </div>
            </div>

            <div class="row">
                <section class="card">
                    <h2>Mest solgte retter</h2>
                    ${topItems.length === 0 ? html`<p class="empty">Ingen retter solgt endnu.</p>` : html`
                        <table>
                            <thead><tr><th>Ret</th><th>Antal</th><th>Omsætning</th></tr></thead>
                            <tbody>
                                ${topItems.map(([name, d]) => html`
                                    <tr>
                                        <td>${name}</td>
                                        <td>${d.qty} stk.</td>
                                        <td>${this._dkk(d.revenue)} kr.</td>
                                    </tr>
                                `)}
                            </tbody>
                        </table>
                    `}
                </section>

                <section class="card">
                    <h2>Bestillinger pr. klasse</h2>
                    ${byClass.length === 0 ? html`<p class="empty">Ingen data endnu.</p>` : html`
                        <table>
                            <thead><tr><th>Klasse</th><th>Ordrer</th><th>Omsætning</th></tr></thead>
                            <tbody>
                                ${byClass.map(([cls, d]) => html`
                                    <tr>
                                        <td>${cls}</td>
                                        <td>${d.count}</td>
                                        <td>${this._dkk(d.revenue)} kr.</td>
                                    </tr>
                                `)}
                            </tbody>
                        </table>
                    `}
                </section>
            </div>

            <section class="card">
                <h2>Seneste bestillinger</h2>
                ${recent.length === 0 ? html`<p class="empty">Ingen bestillinger endnu.</p>` : html`
                    <table>
                        <thead><tr><th>#</th><th>Barn</th><th>Klasse</th><th>Mobil</th><th>Total</th><th>Status</th><th>Tidspunkt</th></tr></thead>
                        <tbody>
                            ${recent.map(o => html`
                                <tr>
                                    <td>${o.id}</td>
                                    <td>${o.childName}</td>
                                    <td>${o.childClass}</td>
                                    <td>${o.phone}</td>
                                    <td>${this._dkk(o.totalAmount)} kr.</td>
                                    <td><span class="badge" style="background:${STATUS_COLORS[o.status] ?? '#999'}">${STATUS_LABELS[o.status] ?? o.status}</span></td>
                                    <td>${this._fmt(o.createdAt)}</td>
                                </tr>
                            `)}
                        </tbody>
                    </table>
                `}
            </section>
        `;
    }
}

customElements.define('content-stats-dashboard', ContentStatsDashboard);
