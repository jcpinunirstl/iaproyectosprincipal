import http from 'k6/http';
import { sleep } from 'k6';

export default function () {
    let res = http.get('http://localhost:5142');
    console.log("STATUS:", res.status);
    sleep(1);
}
