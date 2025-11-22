<?php
/**
 * Requests Controller
 */

class RequestsController extends Controller
{
    public function create($params = [])
    {
        $employeeModel = new EmployeeModel();
        $systemModel = new ApplicationSystem();
        
        $employees = $employeeModel->getAll(true);
        $systems = $systemModel->getAll(true);
        
        $this->view('requests/create', [
            'employees' => $employees,
            'systems' => $systems,
        ]);
    }
    
    public function store($params = [])
    {
        if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
            $this->redirect('/requests/create');
        }
        
        $requestModel = new AuthorizationRequest();
        $auth = Auth::getInstance();
        
        $data = [
            'user_id' => $auth->getUserId() ?? 0, // 0 for anonymous
            'service_level' => $_POST['service_level'] ?? AuthorizationRequest::SERVICE_LEVEL_USER,
            'selected_employees' => json_decode($_POST['selected_employees'] ?? '[]', true),
            'selected_systems' => json_decode($_POST['selected_systems'] ?? '[]', true),
            'comments' => $_POST['comments'] ?? null,
            'manager_id' => $_POST['manager_id'] ?? null,
            'status' => AuthorizationRequest::STATUS_DRAFT,
            'disclosure_acknowledged' => isset($_POST['disclosure_acknowledged']),
        ];
        
        $id = $requestModel->create($data);
        
        if ($auth->isLoggedIn()) {
            $this->redirect("/requests/{$id}");
        } else {
            $this->view('requests/success', ['requestId' => $id]);
        }
    }
    
    public function index($params = [])
    {
        $this->requireAuth();
        
        $requestModel = new AuthorizationRequest();
        $auth = Auth::getInstance();
        
        $filters = [];
        if (!$auth->isAdmin()) {
            $filters['user_id'] = $auth->getUserId();
        }
        
        $requests = $requestModel->getAll($filters);
        
        $this->view('requests/index', ['requests' => $requests]);
    }
    
    public function show($params = [])
    {
        $this->requireAuth();
        
        $id = $params['id'] ?? null;
        if (!$id) {
            $this->redirect('/requests');
        }
        
        $requestModel = new AuthorizationRequest();
        $request = $requestModel->findById($id);
        
        if (!$request) {
            http_response_code(404);
            die('Request not found');
        }
        
        // Check permissions
        $auth = Auth::getInstance();
        if (!$auth->isAdmin() && $request['user_id'] != $auth->getUserId()) {
            http_response_code(403);
            die('Access denied');
        }
        
        $history = $requestModel->getHistory($id);
        
        $this->view('requests/show', [
            'request' => $request,
            'history' => $history,
        ]);
    }
    
    public function managerApprove($params = [])
    {
        $this->requireManager();
        
        $id = $params['id'] ?? null;
        if (!$id) {
            $this->redirect('/requests');
        }
        
        $requestModel = new AuthorizationRequest();
        $request = $requestModel->findById($id);
        
        if (!$request) {
            http_response_code(404);
            die('Request not found');
        }
        
        $this->view('requests/manager-approve', ['request' => $request]);
    }
    
    public function managerApproveStore($params = [])
    {
        $this->requireManager();
        
        if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
            $this->redirect('/requests');
        }
        
        $id = $params['id'] ?? null;
        $requestModel = new AuthorizationRequest();
        $request = $requestModel->findById($id);
        
        if (!$request) {
            http_response_code(404);
            die('Request not found');
        }
        
        $approved = isset($_POST['approved']);
        
        $data = [
            'status' => $approved 
                ? AuthorizationRequest::STATUS_PENDING_FINAL_APPROVAL 
                : AuthorizationRequest::STATUS_REJECTED,
            'manager_approved_at' => $approved ? date('Y-m-d H:i:s') : null,
            'rejection_reason' => $approved ? null : ($_POST['rejection_reason'] ?? null),
        ];
        
        $requestModel->addHistory(
            $id,
            $request['status'],
            $data['status'],
            Auth::getInstance()->getUserId(),
            $_POST['comments'] ?? null
        );
        
        $requestModel->update($id, $data);
        
        $this->redirect("/requests/{$id}");
    }
}

